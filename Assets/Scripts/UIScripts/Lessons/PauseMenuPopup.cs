using System;
using UnityEngine;
using UnityEngine.UI;

namespace UIScripts.Lessons
{
    public class PauseMenuPopup : MonoBehaviour
    {
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button quitButton;

        private Action _onResume;
        private Action _onRestart;
        private Action _onQuit;

        public void Init(Action onResume, Action onRestart, Action onQuit)
        {
            _onResume = onResume;
            _onRestart = onRestart;
            _onQuit = onQuit;

            BindButton(resumeButton, HandleResumeClicked);
            BindButton(restartButton, HandleRestartClicked);
            BindButton(quitButton, HandleQuitClicked);
        }

        private static void BindButton(Button button, Action action)
        {
            if (button == null) return;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => action?.Invoke());
        }

        private void HandleResumeClicked() => _onResume?.Invoke();
        private void HandleRestartClicked() => _onRestart?.Invoke();
        private void HandleQuitClicked() => _onQuit?.Invoke();
    }
}
