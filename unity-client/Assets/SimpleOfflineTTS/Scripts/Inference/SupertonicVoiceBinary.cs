using System;
using Unity.InferenceEngine;
using UnityEngine;

namespace SimpleOfflineTTS
{
    [CreateAssetMenu(menuName = "SimpleOfflineTTS/Voices/Voice (Binary)")]
    public sealed class SupertonicVoiceBinary : SupertonicVoice
    {
        [SerializeField, HideInInspector] byte[] _bytes;

        public override bool CreateStyleTensor()
        {
            if (_bytes == null || _bytes.Length != (StyleTtlFloatCount + StyleDpFloatCount) * 4) return false;

            float[] styleTtl = new float[StyleTtlFloatCount];
            float[] styleDp = new float[StyleDpFloatCount];
            int styleTtlBytes = StyleTtlFloatCount * 4;
            Buffer.BlockCopy(_bytes, 0, styleTtl, 0, styleTtlBytes);
            Buffer.BlockCopy(_bytes, styleTtlBytes, styleDp, 0, StyleDpFloatCount * 4);

            _styleTtlTensor = new Tensor<float>(new TensorShape(1, StyleTtlRows, StyleTtlCols), styleTtl);
            _styleDpTensor = new Tensor<float>(new TensorShape(1, StyleDpRows, StyleDpCols), styleDp);
            return true;
        }

        public void SetBytes(byte[] bytes) => _bytes = bytes;

        public void SetDummyBytes() => _bytes = new byte[(StyleTtlFloatCount + StyleDpFloatCount) * 4];
    }
}
