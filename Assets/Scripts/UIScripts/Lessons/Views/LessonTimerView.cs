using TMPro;
using UnityEngine;

namespace UIScripts.Lessons.Views
{
    public class LessonTimerView : MonoBehaviour
    {
        [SerializeField] private TMP_Text timerText;
        private float _elapsedSeconds;
        private bool _isRunning;

        private void Awake()
        {
            if (timerText == null) timerText = GetComponentInChildren<TMP_Text>(true);
            RefreshText();
        }

        private void Update()
        {
            if (!_isRunning) return;
            _elapsedSeconds += Time.unscaledDeltaTime;
            RefreshText();
        }

        public void StartTimer()
        {
            _elapsedSeconds = 0f;
            _isRunning = true;
            RefreshText();
        }

        public void PauseTimer() => _isRunning = false;
        public void ResumeTimer() => _isRunning = true;
        public void StopTimer() => _isRunning = false;

        private void RefreshText()
        {
            if (timerText == null) return;

            int totalSeconds = Mathf.FloorToInt(_elapsedSeconds);
            timerText.text = $"{totalSeconds / 60:00}:{totalSeconds % 60:00}";
        }
    }
}