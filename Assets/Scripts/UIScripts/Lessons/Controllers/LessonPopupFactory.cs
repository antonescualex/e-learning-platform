using System;
using System.Collections;
using System.Collections.Generic;
using Data.StaticData.Lesson;
using Enums;
using Lessons;
using UIScripts.Lessons.Views;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UIScripts.Lessons.Controllers
{
    public sealed class LessonPopupFactory
    {
        private readonly GameObject _textQuestionPrefab;
        private readonly GameObject _shapesQuestionPrefab;
        private readonly GameObject _clockQuestionPrefab;
        private readonly GameObject _syllableDivisionQuestionPrefab;
        private readonly GameObject _readTogetherQuestionPrefab;
        private readonly GameObject _writeCorrectlyQuestionPrefab;
        private readonly GameObject _endLessonPrefab;
        private readonly GameObject _pauseMenuPrefab;
        private readonly ShapeSpriteCatalog _shapeSpriteCatalog;
        private readonly Canvas _canvas;

        private GameObject _currentLessonPopup;
        private GameObject _endLessonPopup;
        private GameObject _pauseMenuPopup;
        private LessonTimerView _lessonTimer;

        public LessonPopupFactory(
            Canvas canvas,
            ShapeSpriteCatalog shapeSpriteCatalog,
            GameObject textQuestionPrefab,
            GameObject shapesQuestionPrefab,
            GameObject clockQuestionPrefab,
            GameObject syllableDivisionQuestionPrefab,
            GameObject readTogetherQuestionPrefab,
            GameObject writeCorrectlyQuestionPrefab,
            GameObject endLessonPrefab,
            GameObject pauseMenuPrefab)
        {
            _canvas = canvas;
            _shapeSpriteCatalog = shapeSpriteCatalog;
            _textQuestionPrefab = textQuestionPrefab;
            _shapesQuestionPrefab = shapesQuestionPrefab;
            _clockQuestionPrefab = clockQuestionPrefab;
            _syllableDivisionQuestionPrefab = syllableDivisionQuestionPrefab;
            _readTogetherQuestionPrefab = readTogetherQuestionPrefab;
            _writeCorrectlyQuestionPrefab = writeCorrectlyQuestionPrefab;
            _endLessonPrefab = endLessonPrefab;
            _pauseMenuPrefab = pauseMenuPrefab;
        }

        public bool HasPauseMenuOpen => _pauseMenuPopup != null;
        public float CurrentLessonElapsedSeconds => _lessonTimer != null ? _lessonTimer.ElapsedSeconds : 0f;

        public bool TryOpenTextChoiceLesson(
            LessonId lessonId,
            List<TextQuestionDefinition> questions,
            Action<int, int> onCompleted,
            Action onBackRequested)
        {
            if (questions == null || questions.Count == 0)
            {
                return false;
            }

            if (!TryCreatePopup(_textQuestionPrefab, "textQuestionPrefab", out GameObject popup))
            {
                return false;
            }

            TextQuestionView view = popup.GetComponent<TextQuestionView>();
            if (view == null)
            {
                Debug.LogWarning("LessonPopupFactory: TextQuestionView is missing on textQuestionPrefab.");
                Object.Destroy(popup);
                return false;
            }

            view.InitRuntime(
                lessonId.ToDisplayName(),
                questions,
                questions.Count,
                onCompleted,
                onBackRequested);

            FinalizeLessonPopup(popup);
            return true;
        }

        public bool TryOpenShapesLesson(
            LessonId lessonId,
            List<ShapeQuestionDefinition> questions,
            Action<int, int> onCompleted,
            Action onBackRequested)
        {
            if (questions == null || questions.Count == 0 || _shapeSpriteCatalog == null)
            {
                return false;
            }

            if (!TryCreatePopup(_shapesQuestionPrefab, "shapesQuestionPrefab", out GameObject popup))
            {
                return false;
            }

            ShapesQuestionView view = popup.GetComponent<ShapesQuestionView>();
            if (view == null)
            {
                Debug.LogWarning("LessonPopupFactory: ShapesQuestionView is missing on shapesQuestionPrefab.");
                Object.Destroy(popup);
                return false;
            }

            view.InitRuntime(
                lessonId.ToDisplayName(),
                questions,
                _shapeSpriteCatalog,
                questions.Count,
                onCompleted,
                onBackRequested);

            FinalizeLessonPopup(popup);
            return true;
        }

        public bool TryOpenClockLesson(
            LessonId lessonId,
            List<ClockQuestionDefinition> questions,
            Action<int, int> onCompleted,
            Action onBackRequested)
        {
            if (questions == null || questions.Count == 0)
            {
                return false;
            }

            if (!TryCreatePopup(_clockQuestionPrefab, "clockQuestionPrefab", out GameObject popup))
            {
                return false;
            }

            ClockQuestionView view = popup.GetComponent<ClockQuestionView>();
            if (view == null)
            {
                Debug.LogWarning("LessonPopupFactory: ClockQuestionView is missing on clockQuestionPrefab.");
                Object.Destroy(popup);
                return false;
            }

            view.InitRuntime(
                lessonId.ToDisplayName(),
                questions,
                questions.Count,
                onCompleted,
                onBackRequested);

            FinalizeLessonPopup(popup);
            return true;
        }

        public bool TryOpenSyllableDivisionLesson(
            LessonId lessonId,
            List<SyllableDivisionQuestionDefinition> questions,
            Action<int, int> onCompleted,
            Action onBackRequested)
        {
            if (questions == null || questions.Count == 0)
            {
                return false;
            }

            if (!TryCreatePopup(_syllableDivisionQuestionPrefab, "syllableDivisionQuestionPrefab", out GameObject popup))
            {
                return false;
            }

            SyllableDivisionQuestionView view = popup.GetComponent<SyllableDivisionQuestionView>();
            if (view == null)
            {
                Debug.LogWarning("LessonPopupFactory: SyllableDivisionQuestionView is missing on syllableDivisionQuestionPrefab.");
                Object.Destroy(popup);
                return false;
            }

            view.InitRuntime(
                lessonId.ToDisplayName(),
                questions,
                onCompleted,
                onBackRequested);

            FinalizeLessonPopup(popup);
            return true;
        }

        public bool TryOpenReadTogetherLesson(
            LessonId lessonId,
            List<ReadTogetherQuestionDefinition> questions,
            Action<int, int> onCompleted,
            Action onBackRequested)
        {
            if (questions == null || questions.Count == 0)
            {
                return false;
            }

            if (!TryCreatePopup(_readTogetherQuestionPrefab, "readTogetherQuestionPrefab", out GameObject popup))
            {
                return false;
            }

            ReadTogetherQuestionView view = popup.GetComponent<ReadTogetherQuestionView>();
            if (view == null)
            {
                view = popup.AddComponent<ReadTogetherQuestionView>();
            }

            view.InitRuntime(
                lessonId.ToDisplayName(),
                questions,
                onCompleted,
                onBackRequested);

            FinalizeLessonPopup(popup);
            return true;
        }

        public bool TryOpenWriteCorrectlyLesson(
            LessonId lessonId,
            List<WriteCorrectlyQuestionDefinition> questions,
            Action<int, int> onCompleted,
            Action onBackRequested)
        {
            if (questions == null || questions.Count == 0)
            {
                return false;
            }

            if (!TryCreatePopup(_writeCorrectlyQuestionPrefab, "writeCorrectlyQuestionPrefab", out GameObject popup))
            {
                return false;
            }

            WriteCorrectlyQuestionView view = popup.GetComponent<WriteCorrectlyQuestionView>();
            if (view == null)
            {
                Debug.LogWarning("LessonPopupFactory: WriteCorrectlyQuestionView is missing on writeCorrectlyQuestionPrefab.");
                Object.Destroy(popup);
                return false;
            }

            view.InitRuntime(
                lessonId.ToDisplayName(),
                questions,
                onCompleted,
                onBackRequested);

            FinalizeLessonPopup(popup);
            return true;
        }

        public bool TryOpenPauseMenu(Action onResume, Action onRestart, Action onQuit)
        {
            if (!TryCreatePopup(_pauseMenuPrefab, "pauseMenuPrefab", out _pauseMenuPopup))
            {
                return false;
            }

            PauseMenuView view = _pauseMenuPopup.GetComponent<PauseMenuView>();
            if (view == null)
            {
                Debug.LogWarning("LessonPopupFactory: PauseMenuView is missing on pauseMenuPrefab.");
                Object.Destroy(_pauseMenuPopup);
                _pauseMenuPopup = null;
                return false;
            }

            view.Init(onResume, onRestart, onQuit);
            PauseCurrentLessonTimer();
            OpenPopup(_pauseMenuPopup);
            return true;
        }

        public bool TryOpenEndLessonPopup(LessonCompletionResult result, Action onRestart, Action onExit)
        {
            if (!TryCreatePopup(_endLessonPrefab, "endLessonPrefab", out _endLessonPopup))
            {
                return false;
            }

            EndLessonView view = _endLessonPopup.GetComponent<EndLessonView>();
            if (view == null)
            {
                Debug.LogWarning("LessonPopupFactory: EndLessonView is missing on endLessonPrefab.");
                Object.Destroy(_endLessonPopup);
                _endLessonPopup = null;
                return false;
            }

            view.Init(result, onRestart, onExit);
            OpenPopup(_endLessonPopup);
            return true;
        }

        public IEnumerator CloseCurrentLessonPopup()
        {
            StopCurrentLessonTimer();
            yield return ClosePopup(_currentLessonPopup);
            _currentLessonPopup = null;
            _lessonTimer = null;
        }

        public IEnumerator ClosePauseMenu()
        {
            yield return ClosePopup(_pauseMenuPopup);
            _pauseMenuPopup = null;
        }

        public IEnumerator CloseEndLessonPopup()
        {
            yield return ClosePopup(_endLessonPopup);
            _endLessonPopup = null;
        }

        public void DestroyPauseMenuImmediate()
        {
            if (_pauseMenuPopup == null)
            {
                return;
            }

            Object.Destroy(_pauseMenuPopup);
            _pauseMenuPopup = null;
        }

        public void PauseCurrentLessonTimer()
        {
            if (_lessonTimer != null)
            {
                _lessonTimer.PauseTimer();
            }
        }

        public void ResumeCurrentLessonTimer()
        {
            if (_lessonTimer != null)
            {
                _lessonTimer.ResumeTimer();
            }
        }

        private bool TryCreatePopup(GameObject prefab, string prefabFieldName, out GameObject popup)
        {
            popup = null;

            if (_canvas == null)
            {
                Debug.LogWarning("LessonPopupFactory: canvas is not assigned.");
                return false;
            }

            if (prefab == null)
            {
                Debug.LogWarning("LessonPopupFactory: " + prefabFieldName + " is not assigned.");
                return false;
            }

            popup = Object.Instantiate(prefab, _canvas.transform);
            popup.transform.SetAsLastSibling();
            return true;
        }

        private void FinalizeLessonPopup(GameObject popup)
        {
            _currentLessonPopup = popup;
            _lessonTimer = popup.GetComponentInChildren<LessonTimerView>(true);

            if (_lessonTimer == null)
            {
                Debug.LogWarning("LessonPopupFactory: LessonTimerView was not found on current lesson popup.");
            }
            else
            {
                _lessonTimer.StartTimer();
            }

            OpenPopup(popup);
        }

        private void StopCurrentLessonTimer()
        {
            if (_lessonTimer != null)
            {
                _lessonTimer.StopTimer();
            }
        }

        private static void OpenPopup(GameObject popup)
        {
            popup.GetComponent<Ricimi.Popup>()?.Open();
        }

        private static IEnumerator ClosePopup(GameObject popup)
        {
            if (popup == null)
            {
                yield break;
            }

            popup.GetComponent<Ricimi.Popup>()?.Close();
            yield return new WaitForSeconds(0.55f);
            Object.Destroy(popup);
        }
    }
}
