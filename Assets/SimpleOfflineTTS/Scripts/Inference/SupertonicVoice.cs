using System;
using Unity.InferenceEngine;
using UnityEngine;

namespace SimpleOfflineTTS
{
    public abstract class SupertonicVoice : ScriptableObject, IDisposable
    {
        public const int StyleTtlRows = 50;
        public const int StyleTtlCols = 256;
        public const int StyleDpRows = 8;
        public const int StyleDpCols = 16;
        public const int StyleTtlFloatCount = StyleTtlRows * StyleTtlCols;
        public const int StyleDpFloatCount = StyleDpRows * StyleDpCols;

        [NonSerialized] protected Tensor<float> _styleTtlTensor;
        [NonSerialized] protected Tensor<float> _styleDpTensor;

        // Called after loading the voice data
        public abstract bool CreateStyleTensor();

        public Tensor<float> GetStyleTtlTensor() => _styleTtlTensor;

        public Tensor<float> GetStyleDpTensor() => _styleDpTensor;

        public virtual void Dispose()
        {
            if (_styleTtlTensor != null)
            {
                _styleTtlTensor.Dispose();
                _styleTtlTensor = null;
            }

            if (_styleDpTensor != null)
            {
                _styleDpTensor.Dispose();
                _styleDpTensor = null;
            }
        }

        protected virtual void OnDisable() => Dispose();
    }
}
