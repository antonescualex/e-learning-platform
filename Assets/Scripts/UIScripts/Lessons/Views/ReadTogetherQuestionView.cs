using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Data.StaticData.Lesson;
using Recognissimo;
using Recognissimo.Components;
using TMPro;
using UIScripts.Bootstrap;
using UnityEngine;
using UnityEngine.UI;

namespace UIScripts.Lessons.Views
{
    public class ReadTogetherQuestionView : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TMP_Text lessonTypeHeadlineText;
        [SerializeField] private TMP_Text questionCounterText;
        [SerializeField] private TMP_Text passageText;
        [SerializeField] private TMP_Text recognizedSpeechText;
        [SerializeField] private Button microphoneButton;
        [SerializeField] private Image microphoneButtonImage;
        [SerializeField] private Button backButton;

        [Header("Recognissimo")]
        [SerializeField] private SpeechRecognizer speechRecognizer;
        [SerializeField] private StreamingAssetsLanguageModelProvider languageModelProvider;
        [SerializeField] private MicrophoneSpeechSource microphoneSpeechSource;

        [Header("Recognition Settings")]
        [SerializeField] private bool biasRecognizerTowardsPromptWords = true;
        [SerializeField] private bool allowUnknownWords = true;
        [SerializeField, Range(0.5f, 1f)] private float minimumAcceptedScore = 0.85f;
        [SerializeField] private float feedbackDelay = 1.2f;
        [SerializeField] private int alternatives = 3;
        [SerializeField] private bool separateVocabularyEntries = true;

        [Header("Visual Feedback")]
        [SerializeField] private Color listeningMicrophoneColor = new Color(0.95f, 0.45f, 0.45f, 1f);

        private readonly List<ReadTogetherQuestionDefinition> _questions = new List<ReadTogetherQuestionDefinition>();
        private readonly StringBuilder _stableRecognizedText = new StringBuilder();

        private Action _onBackRequested;
        private Action<int, int> _onCompleted;
        private Coroutine _advanceCoroutine;
        private string _lessonHeadline;
        private string _partialRecognizedText = string.Empty;
        private int _currentQuestionIndex;
        private int _correctAnswers;
        private bool _isListening;
        private bool _isEvaluating;
        private bool _ignoreRecognizerCallbacks;
        private Color _defaultMicrophoneColor = Color.white;
        private bool _hasDefaultMicrophoneColor;
        private string _lastNonEmptyRecognizedText = string.Empty;

        public void InitRuntime(
            string lessonHeadline,
            IReadOnlyList<ReadTogetherQuestionDefinition> questions,
            Action<int, int> onCompleted,
            Action onBackRequested)
        {
            _lessonHeadline = lessonHeadline;
            _onCompleted = onCompleted;
            _onBackRequested = onBackRequested;
            _currentQuestionIndex = 0;
            _correctAnswers = 0;
            _isListening = false;
            _isEvaluating = false;
            _ignoreRecognizerCallbacks = false;

            _questions.Clear();
            if (questions != null)
            {
                _questions.AddRange(questions.Where(q => q != null && !string.IsNullOrWhiteSpace(q.PassageText)));
            }

            if (!ValidateConfiguration())
            {
                _onCompleted?.Invoke(0, 0);
                return;
            }

            if (_questions.Count == 0)
            {
                Debug.LogWarning("ReadTogetherQuestionView: questions list is empty.");
                _onCompleted?.Invoke(0, 0);
                return;
            }

            microphoneButton.onClick.RemoveListener(OnMicrophoneButtonClicked);
            microphoneButton.onClick.AddListener(OnMicrophoneButtonClicked);

            backButton.onClick.RemoveListener(OnBackClicked);
            backButton.onClick.AddListener(OnBackClicked);

            UnbindRecognizerEvents();
            BindRecognizerEvents();

            speechRecognizer.AutoStart = false;
            speechRecognizer.LanguageModelProvider = languageModelProvider;
            speechRecognizer.SpeechSource = microphoneSpeechSource;
            speechRecognizer.SeparateVocabularyEntries = separateVocabularyEntries;
            speechRecognizer.EnableDetails = false;
            speechRecognizer.Alternatives = Mathf.Max(1, alternatives);

            _defaultMicrophoneColor = microphoneButtonImage != null ? microphoneButtonImage.color : Color.white;
            _hasDefaultMicrophoneColor = microphoneButtonImage != null;

            StopRecognitionImmediate();
            RenderCurrentQuestion();
        }

        private bool ValidateConfiguration()
        {
            if (lessonTypeHeadlineText == null)
            {
                Debug.LogWarning("ReadTogetherQuestionView: lessonTypeHeadlineText is not assigned.");
                return false;
            }

            if (passageText == null)
            {
                Debug.LogWarning("ReadTogetherQuestionView: passageText is not assigned.");
                return false;
            }

            if (questionCounterText == null)
            {
                Debug.LogWarning("ReadTogetherQuestionView: questionCounterText is not assigned.");
                return false;
            }

            if (recognizedSpeechText == null)
            {
                Debug.LogWarning("ReadTogetherQuestionView: recognizedSpeechText is not assigned.");
                return false;
            }


            if (microphoneButton == null)
            {
                Debug.LogWarning("ReadTogetherQuestionView: microphoneButton is not assigned.");
                return false;
            }

            if (backButton == null)
            {
                Debug.LogWarning("ReadTogetherQuestionView: backButton is not assigned.");
                return false;
            }

            if (speechRecognizer == null)
            {
                Debug.LogWarning("ReadTogetherQuestionView: speechRecognizer is not assigned.");
                return false;
            }

            if (languageModelProvider == null)
            {
                Debug.LogWarning("ReadTogetherQuestionView: languageModelProvider is not assigned.");
                return false;
            }

            if (microphoneSpeechSource == null)
            {
                Debug.LogWarning("ReadTogetherQuestionView: microphoneSpeechSource is not assigned.");
                return false;
            }

            return true;
        }

        private void OnDestroy()
        {
            StopAdvanceRoutine();
            StopRecognitionImmediate();
            UnbindRecognizerEvents();

            if (microphoneButton != null)
            {
                microphoneButton.onClick.RemoveListener(OnMicrophoneButtonClicked);
            }

            if (backButton != null)
            {
                backButton.onClick.RemoveListener(OnBackClicked);
            }
        }

        private void BindRecognizerEvents()
        {
            speechRecognizer.Started.AddListener(OnRecognitionStarted);
            speechRecognizer.PartialResultReady.AddListener(OnPartialResultReady);
            speechRecognizer.ResultReady.AddListener(OnResultReady);
            speechRecognizer.Finished.AddListener(OnRecognitionFinished);
            speechRecognizer.InitializationFailed.AddListener(OnRecognitionInitializationFailed);
            speechRecognizer.RuntimeFailed.AddListener(OnRecognitionRuntimeFailed);
        }

        private void UnbindRecognizerEvents()
        {
            if (speechRecognizer == null) return;

            speechRecognizer.Started.RemoveListener(OnRecognitionStarted);
            speechRecognizer.PartialResultReady.RemoveListener(OnPartialResultReady);
            speechRecognizer.ResultReady.RemoveListener(OnResultReady);
            speechRecognizer.Finished.RemoveListener(OnRecognitionFinished);
            speechRecognizer.InitializationFailed.RemoveListener(OnRecognitionInitializationFailed);
            speechRecognizer.RuntimeFailed.RemoveListener(OnRecognitionRuntimeFailed);
        }

        private void RenderCurrentQuestion()
        {
            StopAdvanceRoutine();
            StopRecognitionImmediate();
            ResetRecognitionText();

            if (_currentQuestionIndex >= _questions.Count)
            {
                _onCompleted?.Invoke(_questions.Count, _correctAnswers);
                return;
            }

            ReadTogetherQuestionDefinition current = _questions[_currentQuestionIndex];

            lessonTypeHeadlineText.text = _lessonHeadline;
            questionCounterText.text = "Question " + (_currentQuestionIndex + 1) + "/" + _questions.Count;
            passageText.text = current.PassageText;
            recognizedSpeechText.text = string.Empty;

            ApplyMicrophoneVisual(false);
        }

        private void OnMicrophoneButtonClicked()
        {
            if (_isEvaluating) return;

            if (_isListening)
            {
                StopRecognition();
            }
            else
            {
                StartRecognition();
            }
        }

        private void StartRecognition()
        {
            if (_currentQuestionIndex < 0 || _currentQuestionIndex >= _questions.Count) return;

            if (speechRecognizer.State != SpeechProcessorState.Inactive)
            {
                StopRecognitionImmediate();
            }

            ResetRecognitionText();
            ConfigureVocabulary(_questions[_currentQuestionIndex].PassageText);

            _isListening = true;
            _isEvaluating = false;
            _ignoreRecognizerCallbacks = false;

            ApplyMicrophoneVisual(true);
            recognizedSpeechText.text = string.Empty;

            try
            {
                speechRecognizer.StartProcessing();
            }
            catch (Exception ex)
            {
                _isListening = false;
                ApplyMicrophoneVisual(false);
                recognizedSpeechText.text = ex.Message;
            }
        }

        private void StopRecognition()
        {
            if (!_isListening) return;

            _isListening = false;
            _isEvaluating = true;

            ApplyMicrophoneVisual(false);

            if (speechRecognizer.State != SpeechProcessorState.Inactive)
            {
                speechRecognizer.StopProcessing();
            }
        }

        private void StopRecognitionImmediate()
        {
            _isListening = false;
            _isEvaluating = false;
            _ignoreRecognizerCallbacks = true;

            ApplyMicrophoneVisual(false);

            if (speechRecognizer != null && speechRecognizer.State != SpeechProcessorState.Inactive)
            {
                speechRecognizer.StopProcessing();
            }
        }

        private void ConfigureVocabulary(string expectedPassage)
        {
            if (!biasRecognizerTowardsPromptWords)
            {
                speechRecognizer.Vocabulary = null;
                return;
            }

            string normalized = NormalizeForComparison(expectedPassage);
            string[] words = normalized.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            HashSet<string> uniqueWords = new HashSet<string>(words, StringComparer.OrdinalIgnoreCase);
            List<string> vocabulary = uniqueWords.ToList();
            vocabulary.Sort(StringComparer.OrdinalIgnoreCase);

            if (allowUnknownWords)
            {
                vocabulary.Add("[unk]");
            }

            speechRecognizer.Vocabulary = vocabulary.Count > 0 ? vocabulary : null;
        }

        private void OnRecognitionStarted()
        {
            if (_ignoreRecognizerCallbacks) return;
        }

        private void OnPartialResultReady(PartialResult partialResult)
        {
            if (_ignoreRecognizerCallbacks) return;

            _partialRecognizedText = partialResult.partial ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(_partialRecognizedText))
            {
                _lastNonEmptyRecognizedText = _partialRecognizedText.Trim();
            }

            recognizedSpeechText.text = GetRecognizedText();
        }

        private void OnResultReady(Result result)
        {
            if (_ignoreRecognizerCallbacks) return;

            if (!string.IsNullOrWhiteSpace(result.text))
            {
                string finalText = result.text.Trim();

                if (_stableRecognizedText.Length > 0) _stableRecognizedText.Append(' ');
                _stableRecognizedText.Append(finalText);

                _lastNonEmptyRecognizedText = _stableRecognizedText.ToString().Trim();
            }

            _partialRecognizedText = string.Empty;

            if (_isListening)
            {
                recognizedSpeechText.text = GetRecognizedText();
            }
        }

        private void OnRecognitionFinished()
        {
            if (_ignoreRecognizerCallbacks) return;

            ApplyMicrophoneVisual(false);

            string recognizedText = GetRecognizedText();
            if (string.IsNullOrWhiteSpace(recognizedText))
            {
                _isEvaluating = false;
                recognizedSpeechText.text = "No speech detected.";
                return;
            }

            EvaluateAnswer(recognizedText);
        }

        private void OnRecognitionInitializationFailed(InitializationException exception)
        {
            if (_ignoreRecognizerCallbacks) return;

            _isListening = false;
            _isEvaluating = false;

            ApplyMicrophoneVisual(false);
            recognizedSpeechText.text = exception.Message;
        }

        private void OnRecognitionRuntimeFailed(RuntimeException exception)
        {
            if (_ignoreRecognizerCallbacks) return;

            _isListening = false;
            _isEvaluating = false;

            ApplyMicrophoneVisual(false);
            recognizedSpeechText.text = exception.Message;
        }

        private void EvaluateAnswer(string recognizedText)
        {
            if (_currentQuestionIndex < 0 || _currentQuestionIndex >= _questions.Count)
            {
                _isEvaluating = false;
                return;
            }

            string expectedText = _questions[_currentQuestionIndex].PassageText;
            float score = CalculateSimilarityScore(expectedText, recognizedText);
            bool isCorrect = score >= minimumAcceptedScore;

            if (isCorrect)
            {
                _correctAnswers++;
            }

            AudioManager.Instance?.PlayAnswerFeedback(isCorrect);
            recognizedSpeechText.text = recognizedText.Trim();
            _advanceCoroutine = StartCoroutine(AdvanceAfterFeedback());
        }

        private IEnumerator AdvanceAfterFeedback()
        {
            yield return new WaitForSecondsRealtime(Mathf.Max(0.1f, feedbackDelay));

            _advanceCoroutine = null;
            _isEvaluating = false;
            _currentQuestionIndex++;

            RenderCurrentQuestion();
        }

        private void StopAdvanceRoutine()
        {
            if (_advanceCoroutine == null) return;

            StopCoroutine(_advanceCoroutine);
            _advanceCoroutine = null;
        }

        private void ResetRecognitionText()
        {
            _stableRecognizedText.Clear();
            _partialRecognizedText = string.Empty;
            _lastNonEmptyRecognizedText = string.Empty;
        }

        private string GetRecognizedText()
        {
            string stable = _stableRecognizedText.ToString().Trim();
            string partial = (_partialRecognizedText ?? string.Empty).Trim();

            if (!string.IsNullOrWhiteSpace(stable) && !string.IsNullOrWhiteSpace(partial))
            {
                return stable + " " + partial;
            }

            if (!string.IsNullOrWhiteSpace(stable)) return stable;
            if (!string.IsNullOrWhiteSpace(partial)) return partial;

            return _lastNonEmptyRecognizedText;
        }

        private void ApplyMicrophoneVisual(bool isListening)
        {
            if (microphoneButtonImage == null || !_hasDefaultMicrophoneColor) return;
            microphoneButtonImage.color = isListening ? listeningMicrophoneColor : _defaultMicrophoneColor;
        }

        private void OnBackClicked()
        {
            StopAdvanceRoutine();
            StopRecognitionImmediate();
            _onBackRequested?.Invoke();
        }

        private float CalculateSimilarityScore(string expectedText, string recognizedText)
        {
            string normalizedExpected = NormalizeForComparison(expectedText);
            string normalizedRecognized = NormalizeForComparison(recognizedText);

            if (string.IsNullOrWhiteSpace(normalizedExpected) || string.IsNullOrWhiteSpace(normalizedRecognized))
            {
                return 0f;
            }

            string[] expectedWords = normalizedExpected.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string[] recognizedWords = normalizedRecognized.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            float tokenScore = CalculateSequenceSimilarity(expectedWords, recognizedWords);
            float charScore = CalculateSequenceSimilarity(normalizedExpected, normalizedRecognized);

            return Mathf.Clamp01(tokenScore * 0.7f + charScore * 0.3f);
        }

        private static float CalculateSequenceSimilarity(string[] expected, string[] recognized)
        {
            int maxLength = Mathf.Max(expected.Length, recognized.Length);
            if (maxLength == 0) return 1f;

            int distance = CalculateEditDistance(expected, recognized);
            return 1f - distance / (float)maxLength;
        }

        private static float CalculateSequenceSimilarity(string expected, string recognized)
        {
            int maxLength = Mathf.Max(expected.Length, recognized.Length);
            if (maxLength == 0) return 1f;

            int distance = CalculateEditDistance(expected, recognized);
            return 1f - distance / (float)maxLength;
        }

        private static int CalculateEditDistance(string[] left, string[] right)
        {
            int[,] dp = new int[left.Length + 1, right.Length + 1];

            for (int i = 0; i <= left.Length; i++) dp[i, 0] = i;
            for (int j = 0; j <= right.Length; j++) dp[0, j] = j;

            for (int i = 1; i <= left.Length; i++)
            {
                for (int j = 1; j <= right.Length; j++)
                {
                    int substitutionCost = string.Equals(left[i - 1], right[j - 1], StringComparison.Ordinal) ? 0 : 1;

                    dp[i, j] = Mathf.Min(
                        Mathf.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1),
                        dp[i - 1, j - 1] + substitutionCost);
                }
            }

            return dp[left.Length, right.Length];
        }

        private static int CalculateEditDistance(string left, string right)
        {
            int[,] dp = new int[left.Length + 1, right.Length + 1];

            for (int i = 0; i <= left.Length; i++) dp[i, 0] = i;
            for (int j = 0; j <= right.Length; j++) dp[0, j] = j;

            for (int i = 1; i <= left.Length; i++)
            {
                for (int j = 1; j <= right.Length; j++)
                {
                    int substitutionCost = left[i - 1] == right[j - 1] ? 0 : 1;

                    dp[i, j] = Mathf.Min(
                        Mathf.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1),
                        dp[i - 1, j - 1] + substitutionCost);
                }
            }

            return dp[left.Length, right.Length];
        }

        private static string NormalizeForComparison(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;

            StringBuilder builder = new StringBuilder(value.Length);
            bool previousWasSpace = false;
            string lowercase = value.ToLowerInvariant();

            for (int i = 0; i < lowercase.Length; i++)
            {
                char character = lowercase[i];

                if (char.IsLetterOrDigit(character))
                {
                    builder.Append(character);
                    previousWasSpace = false;
                    continue;
                }

                if (previousWasSpace || builder.Length == 0) continue;

                builder.Append(' ');
                previousWasSpace = true;
            }

            return builder.ToString().Trim();
        }
    }
}
