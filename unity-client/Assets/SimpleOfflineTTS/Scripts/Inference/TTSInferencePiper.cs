using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.InferenceEngine;
using UnityEngine;

namespace SimpleOfflineTTS
{
    public class TTSInferencePiper : IDisposable
    {
        private const double MaxMillisecondsPerFrame = 4.0;

        private readonly Dictionary<ModelAsset, Worker> _modelWorkers;

        private readonly SemaphoreSlim _generateLock = new SemaphoreSlim(1, 1); // ensures one GenerateAudioClip at a time
        private bool _generating = false;

        public TTSInferencePiper()
        {
            _modelWorkers = new Dictionary<ModelAsset, Worker>();
        }

        public bool HasCreatedWorker(ModelAsset modelAsset, BackendType backend)
        {
            if (_modelWorkers.TryGetValue(modelAsset, out Worker worker))
            {
                if (worker.backendType == backend)
                {
                    return true;
                }
            }

            return false;
        }

        public void CreateWorker(ModelAsset modelAsset, BackendType backend)
        {
            if (_modelWorkers.TryGetValue(modelAsset, out Worker existingWorker))
            {
                if (existingWorker.backendType == backend)
                {
                    Debug.LogWarning($"Worker for voice model {modelAsset.name} is already loaded.");
                    return;
                }
                else
                {
                    Debug.Log($"Voice model {modelAsset.name} is already loaded,");
                    Debug.Log($"Re-creating Worker with backend {backend.ToString()}");

                    _modelWorkers.Remove(modelAsset);
                    existingWorker.Dispose();
                }
            }

            Model voiceModel = ModelLoader.Load(modelAsset);
            Worker worker = new Worker(voiceModel, backend);

            _modelWorkers.Add(modelAsset, worker);
        }

        public bool IsGenerating()
        {
            return _generating;
        }

        public async Task<AudioClip> GenerateAudioClip(
            int[] phonemeIds,
            ModelAsset modelAsset,
            PiperModelConfig config,
            BackendType backend)
        {
            // Wait until previous call completes
            await _generateLock.WaitAsync();

            _generating = true;

            try
            {
                // Make sure the model is loaded
                if (!HasCreatedWorker(modelAsset, backend))
                {
                    CreateWorker(modelAsset, backend);
                }

                var worker = _modelWorkers[modelAsset];
                var inputLengthsShape = new TensorShape(1);
                var scalesShape = new TensorShape(3);

                using var scalesTensor = new Tensor<float>(scalesShape, new float[] { 0.667f, 1.0f, 0.8f });
                using var inputTensor = new Tensor<int>(new TensorShape(1, phonemeIds.Length), phonemeIds);
                using var inputLengthsTensor = new Tensor<int>(inputLengthsShape, new int[] { phonemeIds.Length });

                worker.SetInput("input", inputTensor);
                worker.SetInput("input_lengths", inputLengthsTensor);
                worker.SetInput("scales", scalesTensor);

                // Inference over multiple frames
                await ExecuteWorkerOverTimeAsync(worker, MaxMillisecondsPerFrame);

                Tensor<float> outputTensor = null;
                Tensor<float> cpuCopyTensor = null;
                AudioClip audioClip = null;

                try
                {
                    outputTensor = worker.PeekOutput() as Tensor<float>;
                    cpuCopyTensor = await outputTensor.ReadbackAndCloneAsync();

                    var output = cpuCopyTensor.AsReadOnlyNativeArray();
                    var audioBuffer = new List<float>(output.Length);

                    audioBuffer.AddRange(output);

                    audioClip = AudioClip.Create(
                        "TTSAudioClip",
                        audioBuffer.Count,
                        1,
                        config.audio.sample_rate,
                        false
                    );

                    audioClip.SetData(audioBuffer.ToArray(), 0);
                }
                finally
                {
                    if (cpuCopyTensor != null)
                    {
                        cpuCopyTensor.Dispose();
                    }

                    if (outputTensor != null)
                    {
                        outputTensor.Dispose();
                    }
                }

                return audioClip;
            }
            finally
            {
                _generating = false;
                _generateLock.Release();
            }
        }

        private async Task ExecuteWorkerOverTimeAsync(Worker worker, double maxMilliseconds)
        {
            IEnumerator iterator = worker.ScheduleIterable();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

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

        public void Dispose()
        {
            foreach (Worker worker in _modelWorkers.Values)
            {
                worker.Dispose();
            }

            _modelWorkers.Clear();
        }
    }
}