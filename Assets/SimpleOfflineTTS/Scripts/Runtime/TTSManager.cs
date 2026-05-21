using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Unity.InferenceEngine;
using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SimpleOfflineTTS
{
    public class TTSManager : MonoBehaviour
    {
        [SerializeField]
        bool _logging = true;

        [SerializeField]
        BackendType _backend = BackendType.CPU;

        [Header("Piper")]
        [SerializeField]
        TextAsset _cmuDictAsset;

        CMUDict _cmuDict;
        EnglishPhonemiser _englishPhonemiser;
        TTSInferencePiper _ttsInference;

        [Header("Supertonic")]
        [SerializeField] TextAsset _supertonicUnicodeIndexerJson;
        [SerializeField] TextAsset _supertonicTtsConfigJson;

        [SerializeField] ModelAsset _supertonicDurationPredictor;
        [SerializeField] ModelAsset _supertonicTextEncoder;
        [SerializeField] ModelAsset _supertonicVectorEstimator;
        [SerializeField] ModelAsset _supertonicVocoder;

        [SerializeField, Min(1), Tooltip("Maximum normalized character count to send to Supertonic. Longer text is truncated.")]
        int _supertonicMaxTextLength = 1000;

        [SerializeField, Min(1), Tooltip("Maximum characters per Supertonic chunk. Long text is split at sentence or paragraph boundaries.")]
        int _supertonicMaxChunkLength = 300;

        [SerializeField, Min(0), Tooltip("Silence to insert between Supertonic chunks, in seconds.")]
        float _supertonicChunkSilenceDuration = 0.3f;

        [SerializeField] bool _supertonicWarmup = true;

        const int SUPERTONIC_SAMPLE_RATE = 44100;

        SupertonicCharTokenizer _supertonicTokenizer;
        TTSInferenceSupertonic _supertonicInference;
        int _supertonicSampleRate = SUPERTONIC_SAMPLE_RATE;

        static readonly Regex SupertonicLanguageWrapperRegex = new Regex(@"^\s*<([a-z]{2})>(.*)</\1>\s*$", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);
        static readonly Regex SentenceBoundaryRegex = new Regex(@"(?<=[.!?…])\s+", RegexOptions.Compiled);

        BackendType _currentBackend;
        bool _initialised;

        async void Start()
        {
            // Piper init (if provided)
            if (_cmuDictAsset != null)
            {
                _cmuDict = new CMUDict(_cmuDictAsset);
                _englishPhonemiser = new EnglishPhonemiser();
                _ttsInference = new TTSInferencePiper();
            }

            // Supertonic init (if provided)
            if (_supertonicUnicodeIndexerJson != null)
            {
                _supertonicSampleRate = LoadSupertonicSampleRate(_supertonicTtsConfigJson);
                _supertonicTokenizer = SupertonicCharTokenizer.LoadFromJson(_supertonicUnicodeIndexerJson, _supertonicMaxTextLength);

                _supertonicInference = new TTSInferenceSupertonic();
            }

            _currentBackend = _backend;

            await Initialise();
        }

        async Task Initialise()
        {
            if (_supertonicInference != null)
            {
                _supertonicInference.CreateWorkers(
                    _supertonicDurationPredictor,
                    _supertonicTextEncoder,
                    _supertonicVectorEstimator,
                    _supertonicVocoder,
                    _currentBackend);

                if (_supertonicWarmup)
                {
                    DebugLog("Warming up Supertonic models...");
                    var warmupSw = System.Diagnostics.Stopwatch.StartNew();

                    using SupertonicVoiceBinary dummyVoice = ScriptableObject.CreateInstance<SupertonicVoiceBinary>();
                    dummyVoice.SetDummyBytes();
                    dummyVoice.CreateStyleTensor();
                    await TextToSpeechSupertonic("Hello world", dummyVoice, 1, 1.0f);

                    DebugLog($"Supertonic warmup completed in {warmupSw.ElapsedMilliseconds} ms");
                }
            }

            _initialised = true;
        }

        public bool IsReady()
        {
            return _initialised;
        }

        public bool IsBusy()
        {
            if (_ttsInference != null && _ttsInference.IsGenerating())
            {
                return true;
            }

            if (_supertonicInference != null && _supertonicInference.IsGenerating())
            {
                return true;
            }

            return false;
        }

        public BackendType GetCurrentBackend()
        {
            return _currentBackend;
        }

        public async Task SetBackendType(BackendType backend)
        {
            DebugLog($"Switching backend to {backend.ToString()}");
            _currentBackend = backend;

            await Initialise();
        }

        public async Task<AudioClip> TextToSpeechPiper(string text, ModelAsset modelAsset, PiperModelConfig config)
        {
            if (!_ttsInference.HasCreatedWorker(modelAsset, _currentBackend))
            {
                var loadSw = System.Diagnostics.Stopwatch.StartNew();

                DebugLog($"Loading model: {modelAsset.name}");
                _ttsInference.CreateWorker(modelAsset, _currentBackend);
                DebugLog($"Loaded model and created worker in {loadSw.ElapsedMilliseconds} ms");
            }

            var phonemisationStopwatch = System.Diagnostics.Stopwatch.StartNew();
#if UNITY_WEBGL
            int[] phonemeIds = _englishPhonemiser.TextToPhonemeIds(_cmuDict, config, text);
#else
            int[] phonemeIds = await Task.Run(() => _englishPhonemiser.TextToPhonemeIds(_cmuDict, config, text));
#endif
            DebugLog($"Phonemised text in {phonemisationStopwatch.ElapsedMilliseconds} ms");

            var inferenceStopwatch = System.Diagnostics.Stopwatch.StartNew();
            AudioClip generatedClip = await _ttsInference.GenerateAudioClip(phonemeIds, modelAsset, config, _currentBackend);
            DebugLog($"Inferred audio in {inferenceStopwatch.ElapsedMilliseconds} ms");

            return generatedClip;
        }

        public async Task<AudioClip> TextToSpeechSupertonic(string text, SupertonicVoice voice, int steps = 10, float speed = 1.0f)
        {
            if (_supertonicInference == null)
            {
                DebugLog("Supertonic not initialised - set the models and config in the Inspector.");
                return null;
            }

            List<string> chunks = SplitSupertonicText(text, _supertonicMaxChunkLength);
            if (chunks.Count > 1)
            {
                DebugLog($"Split Supertonic text into {chunks.Count} chunks.");
            }

            if (chunks.Count == 1)
            {
                return await TextToSpeechSupertonicChunk(chunks[0], voice, steps, speed);
            }

            List<AudioClip> clips = new List<AudioClip>(chunks.Count);
            foreach (string chunk in chunks)
            {
                AudioClip clip = await TextToSpeechSupertonicChunk(chunk, voice, steps, speed);
                if (clip != null)
                {
                    clips.Add(clip);
                }
            }

            return ConcatenateClips(clips, _supertonicSampleRate, _supertonicChunkSilenceDuration);
        }

        async Task<AudioClip> TextToSpeechSupertonicChunk(string text, SupertonicVoice voice, int steps, float speed)
        {
            var tokenisationStopwatch = System.Diagnostics.Stopwatch.StartNew();

            int[] inputIds = null;
            float[] textMask = null;

            string normalizedText = _supertonicTokenizer.Normalize(text);
            if (normalizedText.Length > _supertonicTokenizer.ModelMaxLength)
            {
                Debug.LogWarning(
                    $"Supertonic text is {normalizedText.Length} characters after normalization, which exceeds the maximum of {_supertonicTokenizer.ModelMaxLength}. The text will be truncated.",
                    this);
            }

            int seqLen = Mathf.Clamp(normalizedText.Length, 1, _supertonicTokenizer.ModelMaxLength);

#if UNITY_WEBGL
            _supertonicTokenizer.EncodeWithMask(text, _supertonicTokenizer.ModelMaxLength, out inputIds, out textMask);
#else
            await Task.Run(() =>
            {
                _supertonicTokenizer.EncodeWithMask(text, seqLen, out inputIds, out textMask);
            });
#endif
            DebugLog($"Tokenised text in {tokenisationStopwatch.ElapsedMilliseconds} ms");

            var inferenceStopwatch = System.Diagnostics.Stopwatch.StartNew();
            AudioClip clip = await _supertonicInference.GenerateAudioClip(
                inputIds,
                textMask,
                voice,
                steps,
                _supertonicSampleRate,
                speed);

            DebugLog($"Inferred audio in {inferenceStopwatch.ElapsedMilliseconds} ms");

            return clip;
        }

        List<string> SplitSupertonicText(string text, int maxChunkLength)
        {
            List<string> chunks = new List<string>();
            if (string.IsNullOrWhiteSpace(text))
            {
                chunks.Add(text);
                return chunks;
            }

            maxChunkLength = Mathf.Max(1, maxChunkLength);

            string openTag = "";
            string closeTag = "";
            string body = text;

            Match wrapperMatch = SupertonicLanguageWrapperRegex.Match(text);
            if (wrapperMatch.Success)
            {
                string languageCode = wrapperMatch.Groups[1].Value;
                openTag = "<" + languageCode + ">";
                closeTag = "</" + languageCode + ">";
                body = wrapperMatch.Groups[2].Value;
            }

            foreach (string paragraph in Regex.Split(body, @"\r\n|\r|\n"))
            {
                AddParagraphChunks(paragraph, openTag, closeTag, maxChunkLength, chunks);
            }

            if (chunks.Count == 0)
            {
                chunks.Add(text);
            }

            return chunks;
        }

        void AddParagraphChunks(string paragraph, string openTag, string closeTag, int maxChunkLength, List<string> chunks)
        {
            paragraph = paragraph.Trim();
            if (string.IsNullOrEmpty(paragraph))
            {
                return;
            }

            StringBuilder current = new StringBuilder(maxChunkLength);
            string[] sentences = SentenceBoundaryRegex.Split(paragraph);
            foreach (string sentence in sentences)
            {
                AddSentenceToChunks(sentence.Trim(), openTag, closeTag, maxChunkLength, current, chunks);
            }

            FlushChunk(openTag, closeTag, current, chunks);
        }

        void AddSentenceToChunks(string sentence, string openTag, string closeTag, int maxChunkLength, StringBuilder current, List<string> chunks)
        {
            if (string.IsNullOrEmpty(sentence))
            {
                return;
            }

            if (sentence.Length > maxChunkLength)
            {
                FlushChunk(openTag, closeTag, current, chunks);
                AddLongSentenceChunks(sentence, openTag, closeTag, maxChunkLength, chunks);
                return;
            }

            int separatorLength = current.Length > 0 ? 1 : 0;
            if (current.Length + separatorLength + sentence.Length > maxChunkLength)
            {
                FlushChunk(openTag, closeTag, current, chunks);
            }

            if (current.Length > 0)
            {
                current.Append(' ');
            }

            current.Append(sentence);
        }

        void AddLongSentenceChunks(string sentence, string openTag, string closeTag, int maxChunkLength, List<string> chunks)
        {
            StringBuilder current = new StringBuilder(maxChunkLength);
            string[] words = sentence.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);

            foreach (string word in words)
            {
                int separatorLength = current.Length > 0 ? 1 : 0;
                if (current.Length + separatorLength + word.Length > maxChunkLength)
                {
                    FlushChunk(openTag, closeTag, current, chunks);
                }

                if (current.Length > 0)
                {
                    current.Append(' ');
                }

                current.Append(word);
            }

            FlushChunk(openTag, closeTag, current, chunks);
        }

        void FlushChunk(string openTag, string closeTag, StringBuilder current, List<string> chunks)
        {
            if (current.Length == 0)
            {
                return;
            }

            chunks.Add(openTag + current.ToString() + closeTag);
            current.Clear();
        }

        AudioClip ConcatenateClips(List<AudioClip> clips, int sampleRate, float silenceDuration)
        {
            if (clips.Count == 0)
            {
                return null;
            }

            if (clips.Count == 1)
            {
                return clips[0];
            }

            int silenceSamples = Mathf.Max(0, Mathf.RoundToInt(silenceDuration * sampleRate));
            int totalSamples = silenceSamples * (clips.Count - 1);
            foreach (AudioClip clip in clips)
            {
                totalSamples += clip.samples;
            }

            float[] combined = new float[totalSamples];
            int offset = 0;
            foreach (AudioClip clip in clips)
            {
                float[] samples = new float[clip.samples * clip.channels];
                clip.GetData(samples, 0);
                System.Array.Copy(samples, 0, combined, offset, clip.samples);
                offset += clip.samples + silenceSamples;
            }

            AudioClip combinedClip = AudioClip.Create("SupertonicTTS", combined.Length, 1, sampleRate, false);
            combinedClip.SetData(combined, 0);
            return combinedClip;
        }

        private void OnDestroy()
        {
            if (_ttsInference != null)
            {
                _ttsInference.Dispose();
            }

            if (_supertonicInference != null)
            {
                _supertonicInference.Dispose();
            }
        }

        void DebugLog(string log)
        {
            if (_logging)
            {
                Debug.Log(log, this);
            }
        }

        static int LoadSupertonicSampleRate(TextAsset ttsConfigJson)
        {
            if (ttsConfigJson == null || string.IsNullOrWhiteSpace(ttsConfigJson.text))
            {
                return SUPERTONIC_SAMPLE_RATE;
            }

            JObject root = JObject.Parse(ttsConfigJson.text);
            JToken sampleRate = root["ae"]?["sample_rate"];
            if (sampleRate != null && sampleRate.Type == JTokenType.Integer)
            {
                int value = sampleRate.Value<int>();
                if (value > 0)
                {
                    return value;
                }
            }

            return SUPERTONIC_SAMPLE_RATE;
        }
    }
}
