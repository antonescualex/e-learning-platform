using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Unity.InferenceEngine;
using UnityEngine;

namespace SimpleOfflineTTS
{
    public sealed class TTSInferenceSupertonic : IDisposable
    {
        private const int ChunkCompressFactor = 6;
        private const int BaseChunkSize = 512;
        private const int LatentSize = BaseChunkSize * ChunkCompressFactor;
        private const int LatentChannels = 24 * ChunkCompressFactor;

        private const int WebGLYieldInterval = 100;
        private const double MaxMillisecondsPerFrame = 4.0;

        Worker _durationPredictorWorker;
        Worker _textEncoderWorker;
        Worker _vectorEstimatorWorker;
        Worker _vocoderWorker;

        readonly SemaphoreSlim _generateLock = new SemaphoreSlim(1, 1); // ensures one GenerateAudioClip at a time
        bool _generating = false;

        public bool HasCreatedWorkers(
            ModelAsset durationPredictor,
            ModelAsset textEncoder,
            ModelAsset vectorEstimator,
            ModelAsset vocoder,
            BackendType backend)
        {
            return _durationPredictorWorker?.backendType == backend &&
                   _textEncoderWorker?.backendType == backend &&
                   _vectorEstimatorWorker?.backendType == backend &&
                   _vocoderWorker?.backendType == backend;
        }

        public void CreateWorkers(
            ModelAsset durationPredictor,
            ModelAsset textEncoder,
            ModelAsset vectorEstimator,
            ModelAsset vocoder,
            BackendType backend)
        {
            DisposeWorkers();

            _durationPredictorWorker = new Worker(ModelLoader.Load(durationPredictor), backend);
            _textEncoderWorker = new Worker(ModelLoader.Load(textEncoder), backend);
            _vectorEstimatorWorker = new Worker(ModelLoader.Load(vectorEstimator), backend);
            _vocoderWorker = new Worker(ModelLoader.Load(vocoder), backend);
        }

        public bool IsGenerating()
        {
            return _generating;
        }

        public async Task<AudioClip> GenerateAudioClip(
            int[] inputIds,
            float[] textMask,
            SupertonicVoice voice,
            int numInferenceSteps = 5,
            int sampleRate = 44100,
            float speed = 1.0f)
        {
            // Wait until previous call completes
            await _generateLock.WaitAsync();

            _generating = true;

            if (inputIds == null)
            {
                throw new ArgumentNullException(nameof(inputIds));
            }

            if (textMask == null)
            {
                throw new ArgumentNullException(nameof(textMask));
            }

            if (voice == null)
            {
                throw new ArgumentNullException(nameof(voice));
            }

            numInferenceSteps = Math.Max(1, numInferenceSteps);
            sampleRate = Math.Max(1, sampleRate);
            speed = Math.Max(0.1f, speed);

            // Owned Tensors
            Tensor<int> idsTensor = null;
            Tensor<float> textMaskTensor = null;
            Tensor<float> latentMask = null;
            Tensor<float> timestepTensor = null;
            Tensor<float> stepsTensor = null;
            Tensor noisyLatents = null;
            Tensor nextLatents = null;

            try
            {
                idsTensor = new Tensor<int>(new TensorShape(1, inputIds.Length), inputIds);
                textMaskTensor = new Tensor<float>(new TensorShape(1, 1, textMask.Length), textMask);

                // 1) Duration predictor
                _durationPredictorWorker.SetInput("text_ids", idsTensor);
                _durationPredictorWorker.SetInput("style_dp", voice.GetStyleDpTensor());
                _durationPredictorWorker.SetInput("text_mask", textMaskTensor);

                await ExecuteWorkerOverTimeAsync(_durationPredictorWorker, MaxMillisecondsPerFrame);

                float durationSeconds;
                using (var durationReadback = await _durationPredictorWorker.PeekOutput("duration").ReadbackAndCloneAsync() as Tensor<float>)
                {
                    durationSeconds = durationReadback[0] / speed;
                }

                int durationSamples = Mathf.Max(1, Mathf.RoundToInt(durationSeconds * sampleRate));
                int latentLengthChunks = Math.Max(1, (durationSamples + LatentSize - 1) / LatentSize);
                int maxLen = Mathf.Clamp(latentLengthChunks, 1, 100000);

                // 2) Text encoder
                _textEncoderWorker.SetInput("text_ids", idsTensor);
                _textEncoderWorker.SetInput("style_ttl", voice.GetStyleTtlTensor());
                _textEncoderWorker.SetInput("text_mask", textMaskTensor);

                await ExecuteWorkerOverTimeAsync(_textEncoderWorker, MaxMillisecondsPerFrame);

                Tensor textEmb = _textEncoderWorker.PeekOutput("text_emb");

                // 3) Prepare vector-estimator inputs
                latentMask = new Tensor<float>(new TensorShape(1, 1, maxLen));
                for (int i = 0; i < maxLen; i++)
                {
                    latentMask[0, 0, i] = (i < latentLengthChunks) ? 1.0f : 0.0f;
                }

                float[] noiseData = await GenerateMaskedNoiseAsync(LatentChannels, maxLen, latentLengthChunks);

                // Pre-allocate ping-pong buffers
                noisyLatents = new Tensor<float>(new TensorShape(1, LatentChannels, maxLen), noiseData);
                nextLatents = new Tensor<float>(new TensorShape(1, LatentChannels, maxLen));

                timestepTensor = new Tensor<float>(new TensorShape(1));
                stepsTensor = new Tensor<float>(new TensorShape(1));
                stepsTensor.Upload(new float[] { (float)numInferenceSteps });

                float[] timestepArr = new float[1];

                // 4) Flow-matching loop
                for (int step = 0; step < numInferenceSteps; step++)
                {
                    timestepArr[0] = (float)step;
                    timestepTensor.Upload(timestepArr);

                    _vectorEstimatorWorker.SetInput("noisy_latent", noisyLatents);
                    _vectorEstimatorWorker.SetInput("text_emb", textEmb);
                    _vectorEstimatorWorker.SetInput("style_ttl", voice.GetStyleTtlTensor());
                    _vectorEstimatorWorker.SetInput("latent_mask", latentMask);
                    _vectorEstimatorWorker.SetInput("text_mask", textMaskTensor);
                    _vectorEstimatorWorker.SetInput("current_step", timestepTensor);
                    _vectorEstimatorWorker.SetInput("total_step", stepsTensor);

                    await ExecuteWorkerOverTimeAsync(_vectorEstimatorWorker, MaxMillisecondsPerFrame);

                    // CopyOutput() to avoid CPU readbacks inside the loop.
                    _vectorEstimatorWorker.CopyOutput("denoised_latent", ref nextLatents);

                    // Ping-pong references
                    Tensor temp = noisyLatents;
                    noisyLatents = nextLatents;
                    nextLatents = temp;
                }

                // 5) Vocoder
                _vocoderWorker.SetInput("latent", noisyLatents);

                await ExecuteWorkerOverTimeAsync(_vocoderWorker, MaxMillisecondsPerFrame);

                // Move the audio from backend to CPU
                AudioClip clip;
                using (Tensor<float> waveform = await _vocoderWorker.PeekOutput("wav_tts").ReadbackAndCloneAsync() as Tensor<float>)
                {
                    float[] allSamples = waveform.DownloadToArray();

                    int expectedSamples = Math.Min(durationSamples, allSamples.Length);
                    float[] trimmed = await TrimAndClampAsync(allSamples, expectedSamples);

                    clip = AudioClip.Create("SupertonicTTS", trimmed.Length, 1, sampleRate, false);
                    clip.SetData(trimmed, 0);
                }

                return clip;
            }
            finally
            {
                idsTensor?.Dispose();
                textMaskTensor?.Dispose();
                latentMask?.Dispose();
                timestepTensor?.Dispose();
                stepsTensor?.Dispose();
                noisyLatents?.Dispose();
                nextLatents?.Dispose();

                _generating = false;
                _generateLock.Release();
            }
        }

        private async Task ExecuteWorkerOverTimeAsync(Worker worker, double maxMilliseconds)
        {
            IEnumerator iterator = worker.ScheduleIterable();
            Stopwatch stopwatch = Stopwatch.StartNew();

            while (iterator.MoveNext())
            {
                if (stopwatch.Elapsed.TotalMilliseconds >= maxMilliseconds)
                {
                    await Task.Yield();
                    stopwatch.Restart();
                }
            }

            stopwatch.Stop();
        }

        private async Task<float[]> GenerateMaskedNoiseAsync(int channels, int maxLen, int latentLengthChunks)
        {
#if UNITY_WEBGL
            float[] noise = new float[channels * maxLen];
            System.Random rng = new System.Random();
            bool hasSpare = false; 
            float spare = 0.0f;
            
            for (int l = 0; l < latentLengthChunks; l++)
            {
                if (l % WebGLYieldInterval == 0)
                {
                    await Task.Yield();
                }
                
                for (int ch = 0; ch < channels; ch++)
                {
                    if (hasSpare) 
                    { 
                        noise[(ch * maxLen) + l] = spare; 
                        hasSpare = false; 
                    }
                    else
                    {
                        double u1 = 1.0 - rng.NextDouble(); 
                        double u2 = 1.0 - rng.NextDouble();
                        double radius = Math.Sqrt(-2.0 * Math.Log(u1)); 
                        double theta = 2.0 * Math.PI * u2;
                        
                        noise[(ch * maxLen) + l] = (float)(radius * Math.Cos(theta));
                        spare = (float)(radius * Math.Sin(theta)); 
                        hasSpare = true;
                    }
                }
            }
            return noise;
#else
            return await Task.Run(() => GenerateMaskedNoiseInternal(channels, maxLen, latentLengthChunks));
#endif
        }

        private float[] GenerateMaskedNoiseInternal(int channels, int maxLen, int latentLengthChunks)
        {
            float[] noise = new float[channels * maxLen];
            System.Random rng = new System.Random();
            bool hasSpare = false;
            float spare = 0.0f;

            for (int l = 0; l < latentLengthChunks; l++)
            {
                for (int ch = 0; ch < channels; ch++)
                {
                    if (hasSpare)
                    {
                        noise[(ch * maxLen) + l] = spare;
                        hasSpare = false;
                    }
                    else
                    {
                        double u1 = 1.0 - rng.NextDouble();
                        double u2 = 1.0 - rng.NextDouble();
                        double radius = Math.Sqrt(-2.0 * Math.Log(u1));
                        double theta = 2.0 * Math.PI * u2;

                        noise[(ch * maxLen) + l] = (float)(radius * Math.Cos(theta));
                        spare = (float)(radius * Math.Sin(theta));
                        hasSpare = true;
                    }
                }
            }
            return noise;
        }

        private async Task<float[]> TrimAndClampAsync(float[] allSamples, int expectedSamples)
        {
#if UNITY_WEBGL
            float[] trimmed = new float[expectedSamples];
            for (int i = 0; i < expectedSamples; i++)
            {
                if (i % 5000 == 0) 
                {
                    await Task.Yield();
                }
                
                float v = allSamples[i];
                trimmed[i] = v > 1.0f ? 1.0f : (v < -1.0f ? -1.0f : v);
            }
            return trimmed;
#else
            return await Task.Run(() => {
                float[] result = new float[expectedSamples];
                for (int i = 0; i < expectedSamples; i++)
                {
                    float v = allSamples[i];
                    result[i] = v > 1.0f ? 1.0f : (v < -1.0f ? -1.0f : v);
                }
                return result;
            });
#endif
        }

        private void DisposeWorkers()
        {
            if (_durationPredictorWorker != null)
            {
                _durationPredictorWorker.Dispose();
                _durationPredictorWorker = null;
            }

            if (_textEncoderWorker != null)
            {
                _textEncoderWorker.Dispose();
                _textEncoderWorker = null;
            }

            if (_vectorEstimatorWorker != null)
            {
                _vectorEstimatorWorker.Dispose();
                _vectorEstimatorWorker = null;
            }

            if (_vocoderWorker != null)
            {
                _vocoderWorker.Dispose();
                _vocoderWorker = null;
            }
        }

        public void Dispose()
        {
            DisposeWorkers();
        }
    }
}
