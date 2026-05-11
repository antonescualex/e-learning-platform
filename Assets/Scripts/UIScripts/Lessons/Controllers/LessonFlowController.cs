using System;
using System.Collections;
using Enums;
using Lessons;
using Services.Interfaces;
using UnityEngine;

namespace UIScripts.Lessons.Controllers
{
    public sealed class LessonFlowController
    {
        private readonly ILessonService _lessonService;
        private readonly IProfileService _profileService;
        private readonly LessonPopupFactory _popupFactory;
        private readonly Action<IEnumerator> _startCoroutine;
        private readonly Action<string> _loadScene;

        private LessonCompletionResult _lastCompletionResult;
        private bool _isTransitioning;

        public LessonFlowController(
            ILessonService lessonService,
            IProfileService profileService,
            LessonPopupFactory popupFactory,
            Action<IEnumerator> startCoroutine,
            Action<string> loadScene)
        {
            _lessonService = lessonService;
            _profileService = profileService;
            _popupFactory = popupFactory;
            _startCoroutine = startCoroutine;
            _loadScene = loadScene;
        }

        public void HandleBackRequested()
        {
            if (_isTransitioning || _popupFactory.HasPauseMenuOpen)
            {
                return;
            }

            if (!_popupFactory.TryOpenPauseMenu(OnPauseResumeRequested, OnPauseRestartRequested, OnPauseQuitRequested))
            {
                _startCoroutine?.Invoke(CancelAndExit());
            }
        }

        public void HandleLessonCompleted(int totalQuestions, int correctAnswers)
        {
            if (_isTransitioning)
            {
                return;
            }

            _startCoroutine?.Invoke(CompleteAndShowResults(totalQuestions, correctAnswers));
        }

        public IEnumerator CancelAndExit()
        {
            _isTransitioning = true;
            _lastCompletionResult = null;
            _lessonService?.CancelLesson();
            yield return ExitToMainMenu();
        }

        private void OnPauseResumeRequested()
        {
            if (_isTransitioning || !_popupFactory.HasPauseMenuOpen)
            {
                return;
            }

            _startCoroutine?.Invoke(ClosePauseAndResume());
        }

        private IEnumerator ClosePauseAndResume()
        {
            _isTransitioning = true;

            yield return _popupFactory.ClosePauseMenu();
            _popupFactory.ResumeCurrentLessonTimer();

            _isTransitioning = false;
        }

        private void OnPauseRestartRequested()
        {
            if (_isTransitioning || !_popupFactory.HasPauseMenuOpen)
            {
                return;
            }

            _startCoroutine?.Invoke(RestartLessonFromPause());
        }

        private IEnumerator RestartLessonFromPause()
        {
            _isTransitioning = true;

            if (_lessonService == null || !_lessonService.TryGetActiveLesson(out LessonId lessonId))
            {
                _popupFactory.DestroyPauseMenuImmediate();
                yield return CancelAndExit();
                yield break;
            }

            _popupFactory.DestroyPauseMenuImmediate();
            _lastCompletionResult = null;
            _lessonService.StartLesson(lessonId);
            _loadScene?.Invoke("LessonScene");
        }

        private void OnPauseQuitRequested()
        {
            if (_isTransitioning || !_popupFactory.HasPauseMenuOpen)
            {
                return;
            }

            _startCoroutine?.Invoke(QuitFromPause());
        }

        private IEnumerator QuitFromPause()
        {
            _isTransitioning = true;

            _popupFactory.DestroyPauseMenuImmediate();
            _lastCompletionResult = null;

            if (_lessonService != null)
            {
                bool reported = false;
                string reportError = null;
                float elapsedSeconds = _popupFactory.CurrentLessonElapsedSeconds;

                yield return _lessonService.RegisterIncompleteLesson(
                    elapsedSeconds,
                    () => reported = true,
                    error => reportError = error);

                if (!reported)
                {
                    if (!string.IsNullOrWhiteSpace(reportError))
                    {
                        Debug.LogWarning("Report incomplete lesson failed:\n" + reportError);
                    }

                    _lessonService.CancelLesson();
                }
            }

            yield return ExitToMainMenu();
        }

        private IEnumerator CompleteAndShowResults(int totalQuestions, int correctAnswers)
        {
            _isTransitioning = true;

            if (totalQuestions <= 0)
            {
                _lastCompletionResult = null;
                _lessonService?.CancelLesson();
                yield return ExitToMainMenu();
                yield break;
            }

            if (_lessonService == null)
            {
                _lastCompletionResult = null;
                yield return ExitToMainMenu();
                yield break;
            }

            float elapsedSeconds = _popupFactory.CurrentLessonElapsedSeconds;

            LessonCompletionResult result = null;
            string completionError = null;

            yield return _lessonService.CompleteLesson(
                totalQuestions,
                correctAnswers,
                elapsedSeconds,
                completedResult => result = completedResult,
                error => completionError = error);

            if (result == null)
            {
                if (!string.IsNullOrWhiteSpace(completionError))
                {
                    Debug.LogWarning("Complete lesson failed:\n" + completionError);
                }

                _lastCompletionResult = null;
                yield return ExitToMainMenu();
                yield break;
            }

            _lastCompletionResult = result;

            yield return _popupFactory.CloseCurrentLessonPopup();

            if (!_popupFactory.TryOpenEndLessonPopup(result, OnRestartRequested, OnExitRequested))
            {
                _lastCompletionResult = null;
                _loadScene?.Invoke("MainMenu");
                yield break;
            }

            _isTransitioning = false;
        }

        private void OnRestartRequested()
        {
            if (_isTransitioning || _lastCompletionResult == null)
            {
                return;
            }

            _startCoroutine?.Invoke(RestartLesson());
        }

        private IEnumerator RestartLesson()
        {
            _isTransitioning = true;

            LessonId lessonId = _lastCompletionResult.LessonId;
            yield return _popupFactory.CloseEndLessonPopup();

            _lessonService?.StartLesson(lessonId);
            _lastCompletionResult = null;

            _loadScene?.Invoke("LessonScene");
        }

        private void OnExitRequested()
        {
            if (_isTransitioning)
            {
                return;
            }

            _startCoroutine?.Invoke(ExitFromResults());
        }

        private IEnumerator ExitFromResults()
        {
            _isTransitioning = true;

            yield return _popupFactory.CloseEndLessonPopup();
            _lastCompletionResult = null;

            _loadScene?.Invoke("MainMenu");
        }

        private IEnumerator ExitToMainMenu()
        {
            yield return _popupFactory.CloseCurrentLessonPopup();
            _loadScene?.Invoke("MainMenu");
        }
    }
}
