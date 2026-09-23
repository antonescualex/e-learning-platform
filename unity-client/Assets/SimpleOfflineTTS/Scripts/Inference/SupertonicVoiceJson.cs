using Newtonsoft.Json;
using Unity.InferenceEngine;
using UnityEngine;

namespace SimpleOfflineTTS
{
    [CreateAssetMenu(menuName = "SimpleOfflineTTS/Voices/Voice (JSON)")]
    public sealed class SupertonicVoiceJson : SupertonicVoice
    {
        public TextAsset jsonFile;

        public override bool CreateStyleTensor()
        {
            Dispose();

            if (jsonFile == null) return false;

            // Deserialize the JSON structure
            var data = JsonConvert.DeserializeObject<SupertonicJsonData>(jsonFile.text);
            float[] styleTtl = new float[StyleTtlFloatCount];
            float[] styleDp = new float[StyleDpFloatCount];

            int idx = 0;
            foreach (var row in data.style_ttl.data[0])
                foreach (float val in row) styleTtl[idx++] = val;

            idx = 0;
            foreach (var row in data.style_dp.data[0])
                foreach (float val in row) styleDp[idx++] = val;

            _styleTtlTensor = new Tensor<float>(new TensorShape(1, StyleTtlRows, StyleTtlCols), styleTtl);
            _styleDpTensor = new Tensor<float>(new TensorShape(1, StyleDpRows, StyleDpCols), styleDp);
            return true;
        }

        // JSON helper classes
        private class SupertonicJsonData
        {
            public JsonComponent style_ttl { get; set; }
            public JsonComponent style_dp { get; set; }
        }
        private class JsonComponent
        {
            public float[][][] data { get; set; }
        }
    }
}
