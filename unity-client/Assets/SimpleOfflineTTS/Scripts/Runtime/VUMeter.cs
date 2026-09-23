using UnityEngine;
using UnityEngine.UI;

namespace SimpleOfflineTTS
{
    public class VUMeter : MonoBehaviour
    {
        [Header("Mic Level UI")]
        [SerializeField]
        Image _micLevelFill;

        [SerializeField]
        Color _silentColor = new Color(0.2f, 0.8f, 0.2f);

        [SerializeField]
        Color _speakingColor = new Color(1.0f, 0.6f, 0.2f);

        [SerializeField]
        float _rmsVisualMultiplier = 20.0f;

        [SerializeField]
        float _levelSmoothing = 10.0f;

        float _smoothedMicLevel = 0.0f;

        const int VU_SAMPLES = 1024;
        float[] _buffer;

        public void Reset()
        {
            _smoothedMicLevel = 0.0f;

            _micLevelFill.fillAmount = 0.0f;
        }

        public void SetRMSValue(float rms)
        {
            // Convert RMS to a usable 0-1 range
            float targetLevel = Mathf.Clamp01(rms * _rmsVisualMultiplier);

            // Smooth it so it looks nice
            _smoothedMicLevel = Mathf.Lerp(
                _smoothedMicLevel,
                targetLevel,
                Time.deltaTime * _levelSmoothing);

            // Update UI
            _micLevelFill.fillAmount = _smoothedMicLevel;
            _micLevelFill.color = Color.Lerp(
                _silentColor,
                _speakingColor,
                _smoothedMicLevel);
        }

        public float SetRmsFromSource(AudioSource source)
        {
            if ((source == null) || (!source.isPlaying))
            {
                SetRMSValue(0.0f);
                return 0.0f;
            }

            if (_buffer == null)
            {
                _buffer = new float[VU_SAMPLES];
            }

            source.GetOutputData(_buffer, 0);

            double sumSq = 0.0;
            for (int i = 0; i < _buffer.Length; i++)
            {
                float s = _buffer[i];
                sumSq += s * s;
            }

            float rms = Mathf.Sqrt((float)(sumSq / _buffer.Length));

            SetRMSValue(rms);

            return rms;
        }
    }
}
