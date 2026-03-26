using System.Collections;
using System.Collections.Generic;
using Data.StaticData.Lesson;
using Lessons;
using Services.Interfaces;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UIScripts.Lessons
{
    public class LessonSceneController : MonoBehaviour
    {
        [Header("Popup")]
        [SerializeField] private GameObject textQuestionPrefab;
        [SerializeField] private GameObject shapesQuestionPrefab;
        [SerializeField] private GameObject clockQuestionPrefab;
        [SerializeField] private GameObject syllableDivisionQuestionPrefab;
        [SerializeField] private GameObject readTogetherQuestionPrefab;
        [SerializeField] private GameObject writeCorrectlyQuestionPrefab;
        [SerializeField] private GameObject endLessonPrefab;
        [SerializeField] private GameObject pauseMenuPrefab;
        [SerializeField] private ShapeSpriteCatalog shapeSpriteCatalog;
        [SerializeField] private Canvas canvas;

        [Header("Lesson Data")]
        [SerializeField] private int questionsPerSession = 5;
        [SerializeField] private GameObject loadingOverlay;

        private IProfileService _profileService;
        private ILessonService _lessonService;
        private ILessonContentService _lessonContentService;
        private GameObject _currentPopup;
        private GameObject _endLessonPopup;
        private GameObject _pauseMenuPopup;
        private LessonCompletionResult _lastCompletionResult;
        private bool _isTransitioning;

        private void Start()
        {
            if (App.Instance == null || App.Instance.LessonService == null)
            {
                SceneManager.LoadScene("Bootstrap");
                return;
            }

            _lessonService = App.Instance.LessonService;
            _lessonContentService = App.Instance.LessonContentService;
            _profileService = App.Instance.ProfileService;

            if (!_lessonService.TryGetActiveLesson(out LessonId lessonId))
            {
                SceneManager.LoadScene("MainMenu");
                return;
            }

            StartCoroutine(OpenLessonPopupWithDelay(lessonId));
        }

        private IEnumerator OpenLessonPopupWithDelay(LessonId lessonId)
        {
            yield return new WaitForSeconds(0.15f);

            if (canvas == null)
            {
                Debug.LogWarning("LessonSceneController: canvas is not assigned.");
                yield return CancelAndExit();
                yield break;
            }

            switch (lessonId.GetViewType())
            {
                case LessonViewType.TextChoice:
                    yield return OpenTextChoiceLesson(lessonId);
                    yield break;

                case LessonViewType.GeometricalShapes:
                    yield return OpenShapesLesson(lessonId);
                    yield break;

                case LessonViewType.Clock:
                    yield return OpenClockLesson(lessonId);
                    yield break;

                case LessonViewType.SyllableDivision:
                    yield return OpenSyllableDivisionLesson(lessonId);
                    yield break;

                case LessonViewType.ReadTogether:
                    yield return OpenReadTogetherLesson(lessonId);
                    yield break;

                case LessonViewType.WriteCorrectly:
                    yield return OpenWriteCorrectlyLesson(lessonId);
                    yield break;

                default:
                    yield return CancelAndExit();
                    yield break;
            }
        }

        private IEnumerator OpenTextChoiceLesson(LessonId lessonId)
        {
            if (_lessonContentService == null)
            {
                Debug.LogWarning("Text-choice lesson requires ILessonContentService.");
                yield return CancelAndExit();
                yield break;
            }

            SetLoading(true);

            List<TextMathsQuestionDefinition> generatedQuestions = null;
            string errorMessage = null;

            yield return _lessonContentService.GenerateTextChoiceLesson(
                lessonId,
                Mathf.Max(1, questionsPerSession),
                questions => generatedQuestions = questions,
                error => errorMessage = error);

            SetLoading(false);

            if (generatedQuestions != null && TryOpenRuntimeTextChoiceLesson(lessonId, generatedQuestions))
            {
                yield break;
            }

            Debug.LogWarning("AI text-choice lesson failed: " + errorMessage);
            yield return CancelAndExit();
        }

        private IEnumerator OpenShapesLesson(LessonId lessonId)
        {
            IReadOnlyList<string> allowedShapeIds = shapeSpriteCatalog != null ? shapeSpriteCatalog.GetAllIds() : null;

            if (_lessonContentService == null || allowedShapeIds == null || allowedShapeIds.Count == 0)
            {
                Debug.LogWarning("Shapes lesson requires ILessonContentService and ShapeSpriteCatalog.");
                yield return CancelAndExit();
                yield break;
            }

            SetLoading(true);

            List<ShapeQuestionDefinition> generatedQuestions = null;
            string errorMessage = null;

            yield return _lessonContentService.GenerateShapesLesson(
                lessonId,
                Mathf.Max(1, questionsPerSession),
                allowedShapeIds,
                questions => generatedQuestions = questions,
                error => errorMessage = error);

            SetLoading(false);

            if (generatedQuestions != null && TryOpenRuntimeShapesLesson(lessonId, generatedQuestions))
            {
                yield break;
            }

            Debug.LogWarning("AI shapes lesson failed: " + errorMessage);
            yield return CancelAndExit();
        }


        private bool TryOpenRuntimeShapesLesson(LessonId lessonId, List<ShapeQuestionDefinition> questions)
        {
            if (questions == null || questions.Count == 0 || shapeSpriteCatalog == null) return false;

            if (shapesQuestionPrefab == null)
            {
                Debug.LogWarning("LessonSceneController: shapesQuestionPrefab is not assigned.");
                return false;
            }

            _currentPopup = Instantiate(shapesQuestionPrefab, canvas.transform);
            _currentPopup.transform.SetAsLastSibling();

            ShapesQuestionView view = _currentPopup.GetComponent<ShapesQuestionView>();
            if (view == null)
            {
                Debug.LogWarning("LessonSceneController: ShapesQuestionView is missing on shapesQuestionPrefab.");
                Destroy(_currentPopup);
                _currentPopup = null;
                return false;
            }

            view.InitRuntime(
                lessonId.ToDisplayName(),
                questions,
                shapeSpriteCatalog,
                questions.Count,
                OnLessonCompleted,
                OnBackRequested);

            _currentPopup.GetComponent<Ricimi.Popup>()?.Open();
            return true;
        }

        private IEnumerator OpenClockLesson(LessonId lessonId)
        {
            if (_lessonContentService == null)
            {
                Debug.LogWarning("Clock lesson requires ILessonContentService.");
                yield return CancelAndExit();
                yield break;
            }

            SetLoading(true);

            List<ClockQuestionDefinition> generatedQuestions = null;
            string errorMessage = null;

            yield return _lessonContentService.GenerateClockLesson(
                lessonId,
                Mathf.Max(1, questionsPerSession),
                questions => generatedQuestions = questions,
                error => errorMessage = error);

            SetLoading(false);

            if (generatedQuestions != null && TryOpenRuntimeClockLesson(lessonId, generatedQuestions))
            {
                yield break;
            }

            Debug.LogWarning("AI clock lesson failed: " + errorMessage);
            yield return CancelAndExit();
        }

        private IEnumerator OpenSyllableDivisionLesson(LessonId lessonId)
        {
            if (_lessonContentService == null)
            {
                Debug.LogWarning("Syllable division lesson requires ILessonContentService.");
                yield return CancelAndExit();
                yield break;
            }

            SetLoading(true);

            List<SyllableDivisionQuestionDefinition> generatedQuestions = null;
            string errorMessage = null;

            yield return _lessonContentService.GenerateSyllableDivisionLesson(
                lessonId,
                Mathf.Max(1, questionsPerSession),
                questions => generatedQuestions = questions,
                error => errorMessage = error);

            SetLoading(false);

            if (generatedQuestions != null && TryOpenRuntimeSyllableDivisionLesson(lessonId, generatedQuestions))
            {
                yield break;
            }

            Debug.LogWarning("AI syllable division lesson failed: " + errorMessage);
            yield return CancelAndExit();
        }

        private IEnumerator OpenReadTogetherLesson(LessonId lessonId)
        {
            if (_lessonContentService == null)
            {
                Debug.LogWarning("ReadTogether lesson requires ILessonContentService.");
                yield return CancelAndExit();
                yield break;
            }

            SetLoading(true);

            List<ReadTogetherQuestionDefinition> generatedQuestions = null;
            string errorMessage = null;

            yield return _lessonContentService.GenerateReadTogetherLesson(
                lessonId,
                Mathf.Max(1, questionsPerSession),
                questions => generatedQuestions = questions,
                error => errorMessage = error);

            SetLoading(false);

            if (generatedQuestions != null && TryOpenRuntimeReadTogetherLesson(lessonId, generatedQuestions))
            {
                yield break;
            }

            Debug.LogWarning("AI read-together lesson failed: " + errorMessage);
            yield return CancelAndExit();
        }

        private bool TryOpenRuntimeReadTogetherLesson(LessonId lessonId, List<ReadTogetherQuestionDefinition> questions)
        {
            if (questions == null || questions.Count == 0) return false;

            if (readTogetherQuestionPrefab == null)
            {
                Debug.LogWarning("LessonSceneController: readTogetherQuestionPrefab is not assigned.");
                return false;
            }

            _currentPopup = Instantiate(readTogetherQuestionPrefab, canvas.transform);
            _currentPopup.transform.SetAsLastSibling();

            ReadTogetherQuestionView view = _currentPopup.GetComponent<ReadTogetherQuestionView>();
            if (view == null)
            {
                view = _currentPopup.AddComponent<ReadTogetherQuestionView>();
            }

            view.InitRuntime(
                lessonId.ToDisplayName(),
                questions,
                OnLessonCompleted,
                OnBackRequested);

            _currentPopup.GetComponent<Ricimi.Popup>()?.Open();
            return true;
        }


        private IEnumerator OpenWriteCorrectlyLesson(LessonId lessonId)
        {
            if (_lessonContentService == null)
            {
                Debug.LogWarning("WriteCorrectly lesson requires ILessonContentService.");
                yield return CancelAndExit();
                yield break;
            }

            SetLoading(true);

            List<WriteCorrectlyQuestionDefinition> generatedQuestions = null;
            string errorMessage = null;

            yield return _lessonContentService.GenerateWriteCorrectlyLesson(
                lessonId,
                Mathf.Max(1, questionsPerSession),
                questions => generatedQuestions = questions,
                error => errorMessage = error);

            SetLoading(false);

            if (generatedQuestions != null && TryOpenRuntimeWriteCorrectlyLesson(lessonId, generatedQuestions))
            {
                yield break;
            }

            Debug.LogWarning("AI write-correctly lesson failed: " + errorMessage);
            yield return CancelAndExit();
        }

        private bool TryOpenRuntimeWriteCorrectlyLesson(LessonId lessonId, List<WriteCorrectlyQuestionDefinition> questions)
        {
            if (questions == null || questions.Count == 0) return false;

            if (writeCorrectlyQuestionPrefab == null)
            {
                Debug.LogWarning("LessonSceneController: writeCorrectlyQuestionPrefab is not assigned.");
                return false;
            }

            _currentPopup = Instantiate(writeCorrectlyQuestionPrefab, canvas.transform);
            _currentPopup.transform.SetAsLastSibling();

            WriteCorrectlyQuestionView view = _currentPopup.GetComponent<WriteCorrectlyQuestionView>();
            if (view == null)
            {
                Debug.LogWarning("LessonSceneController: WriteCorrectlyQuestionView is missing on writeCorrectlyQuestionPrefab.");
                Destroy(_currentPopup);
                _currentPopup = null;
                return false;
            }

            view.InitRuntime(
                lessonId.ToDisplayName(),
                questions,
                OnLessonCompleted,
                OnBackRequested);

            _currentPopup.GetComponent<Ricimi.Popup>()?.Open();
            return true;
        }

        private bool TryOpenRuntimeTextChoiceLesson(LessonId lessonId, List<TextMathsQuestionDefinition> questions)
        {
            if (questions == null || questions.Count == 0) return false;

            if (textQuestionPrefab == null)
            {
                Debug.LogWarning("LessonSceneController: textQuestionPrefab is not assigned.");
                return false;
            }

            _currentPopup = Instantiate(textQuestionPrefab, canvas.transform);
            _currentPopup.transform.SetAsLastSibling();

            TextQuestionView view = _currentPopup.GetComponent<TextQuestionView>();
            if (view == null)
            {
                Debug.LogWarning("LessonSceneController: TextQuestionView is missing on textQuestionPrefab.");
                Destroy(_currentPopup);
                _currentPopup = null;
                return false;
            }

            view.InitRuntime(
                lessonId.ToDisplayName(),
                questions,
                questions.Count,
                OnLessonCompleted,
                OnBackRequested);

            _currentPopup.GetComponent<Ricimi.Popup>()?.Open();
            return true;
        }

        private bool TryOpenRuntimeClockLesson(LessonId lessonId, List<ClockQuestionDefinition> questions)
        {
            if (questions == null || questions.Count == 0) return false;

            if (clockQuestionPrefab == null)
            {
                Debug.LogWarning("LessonSceneController: clockQuestionPrefab is not assigned.");
                return false;
            }

            _currentPopup = Instantiate(clockQuestionPrefab, canvas.transform);
            _currentPopup.transform.SetAsLastSibling();

            ClockQuestionView view = _currentPopup.GetComponent<ClockQuestionView>();
            if (view == null)
            {
                Debug.LogWarning("LessonSceneController: ClockQuestionView is missing on clockQuestionPrefab.");
                Destroy(_currentPopup);
                _currentPopup = null;
                return false;
            }

            view.InitRuntime(
                lessonId.ToDisplayName(),
                questions,
                questions.Count,
                OnLessonCompleted,
                OnBackRequested);

            _currentPopup.GetComponent<Ricimi.Popup>()?.Open();
            return true;
        }

        private bool TryOpenRuntimeSyllableDivisionLesson(LessonId lessonId, List<SyllableDivisionQuestionDefinition> questions)
        {
            if (questions == null || questions.Count == 0) return false;

            if (syllableDivisionQuestionPrefab == null)
            {
                Debug.LogWarning("LessonSceneController: syllableDivisionQuestionPrefab is not assigned.");
                return false;
            }

            _currentPopup = Instantiate(syllableDivisionQuestionPrefab, canvas.transform);
            _currentPopup.transform.SetAsLastSibling();

            SyllableDivisionQuestionView view = _currentPopup.GetComponent<SyllableDivisionQuestionView>();
            if (view == null)
            {
                Debug.LogWarning("LessonSceneController: SyllableDivisionQuestionView is missing on syllableDivisionQuestionPrefab.");
                Destroy(_currentPopup);
                _currentPopup = null;
                return false;
            }

            view.InitRuntime(
                lessonId.ToDisplayName(),
                questions,
                OnLessonCompleted,
                OnBackRequested);

            _currentPopup.GetComponent<Ricimi.Popup>()?.Open();
            return true;
        }

        private void OnBackRequested()
        {
            if (_isTransitioning || _pauseMenuPopup != null) return;

            if (!TryOpenPauseMenu())
            {
                StartCoroutine(CancelAndExit());
            }
        }

        private bool TryOpenPauseMenu()
        {
            if (pauseMenuPrefab == null)
            {
                Debug.LogWarning("LessonSceneController: pauseMenuPrefab is not assigned.");
                return false;
            }

            if (canvas == null)
            {
                Debug.LogWarning("LessonSceneController: canvas is not assigned.");
                return false;
            }

            _pauseMenuPopup = Instantiate(pauseMenuPrefab, canvas.transform);
            _pauseMenuPopup.transform.SetAsLastSibling();

            PauseMenuPopup popup = _pauseMenuPopup.GetComponent<PauseMenuPopup>();
            if (popup == null)
            {
                Debug.LogWarning("LessonSceneController: PauseMenuPopup is missing on pauseMenuPrefab.");
                Destroy(_pauseMenuPopup);
                _pauseMenuPopup = null;
                return false;
            }

            popup.Init(OnPauseResumeRequested, OnPauseRestartRequested, OnPauseQuitRequested);
            _pauseMenuPopup.GetComponent<Ricimi.Popup>()?.Open();
            return true;
        }

        private void OnPauseResumeRequested()
        {
            if (_isTransitioning || _pauseMenuPopup == null) return;
            StartCoroutine(ClosePauseAndResume());
        }

        private IEnumerator ClosePauseAndResume()
        {
            _isTransitioning = true;

            yield return ClosePopup(_pauseMenuPopup);
            _pauseMenuPopup = null;

            _isTransitioning = false;
        }

        private void OnPauseRestartRequested()
        {
            if (_isTransitioning || _pauseMenuPopup == null) return;
            StartCoroutine(RestartLessonFromPause());
        }

        private IEnumerator RestartLessonFromPause()
        {
            _isTransitioning = true;

            if (!_lessonService.TryGetActiveLesson(out LessonId lessonId))
            {
                Destroy(_pauseMenuPopup);
                _pauseMenuPopup = null;
                yield return CancelAndExit();
                yield break;
            }

            Destroy(_pauseMenuPopup);
            _pauseMenuPopup = null;

            _lastCompletionResult = null;
            _lessonService.StartLesson(lessonId);
            SceneManager.LoadScene("LessonScene");
        }

        private void OnPauseQuitRequested()
        {
            if (_isTransitioning || _pauseMenuPopup == null) return;
            StartCoroutine(QuitFromPause());
        }

        private IEnumerator QuitFromPause()
        {
            _isTransitioning = true;

            Destroy(_pauseMenuPopup);
            _pauseMenuPopup = null;

            _lastCompletionResult = null;
            _profileService?.RegisterIncompleteLesson();
            _lessonService.CancelLesson();
            yield return ExitToMainMenu();
        }

        private void OnLessonCompleted(int totalQuestions, int correctAnswers)
        {
            if (_isTransitioning) return;
            StartCoroutine(CompleteAndShowResults(totalQuestions, correctAnswers));
        }

        private IEnumerator CompleteAndShowResults(int totalQuestions, int correctAnswers)
        {
            _isTransitioning = true;

            if (totalQuestions <= 0)
            {
                _lastCompletionResult = null;
                _lessonService.CancelLesson();
                yield return ExitToMainMenu();
                yield break;
            }

            LessonCompletionResult result;
            if (!_lessonService.TryCompleteLesson(totalQuestions, correctAnswers, out result))
            {
                _lastCompletionResult = null;
                yield return ExitToMainMenu();
                yield break;
            }

            _lastCompletionResult = result;

            yield return ClosePopup(_currentPopup);
            _currentPopup = null;

            if (!TryOpenEndLessonPopup(result))
            {
                _lastCompletionResult = null;
                SceneManager.LoadScene("MainMenu");
                yield break;
            }

            _isTransitioning = false;
        }

        private bool TryOpenEndLessonPopup(LessonCompletionResult result)
        {
            if (endLessonPrefab == null)
            {
                Debug.LogWarning("LessonSceneController: endLessonPrefab is not assigned.");
                return false;
            }

            if (canvas == null)
            {
                Debug.LogWarning("LessonSceneController: canvas is not assigned.");
                return false;
            }

            _endLessonPopup = Instantiate(endLessonPrefab, canvas.transform);
            _endLessonPopup.transform.SetAsLastSibling();

            EndLessonPopup popup = _endLessonPopup.GetComponent<EndLessonPopup>();
            if (popup == null)
            {
                Debug.LogWarning("LessonSceneController: EndLessonPopup is missing on endLessonPrefab.");
                Destroy(_endLessonPopup);
                _endLessonPopup = null;
                return false;
            }

            popup.Init(result, OnRestartRequested, OnExitRequested);
            _endLessonPopup.GetComponent<Ricimi.Popup>()?.Open();
            return true;
        }

        private void OnRestartRequested()
        {
            if (_isTransitioning || _lastCompletionResult == null) return;
            StartCoroutine(RestartLesson());
        }

        private IEnumerator RestartLesson()
        {
            _isTransitioning = true;

            yield return ClosePopup(_endLessonPopup);
            _endLessonPopup = null;

            _lessonService.StartLesson(_lastCompletionResult.LessonId);
            _lastCompletionResult = null;

            SceneManager.LoadScene("LessonScene");
        }

        private void OnExitRequested()
        {
            if (_isTransitioning) return;
            StartCoroutine(ExitFromResults());
        }

        private IEnumerator ExitFromResults()
        {
            _isTransitioning = true;

            yield return ClosePopup(_endLessonPopup);
            _endLessonPopup = null;
            _lastCompletionResult = null;

            SceneManager.LoadScene("MainMenu");
        }

        private IEnumerator CancelAndExit()
        {
            _isTransitioning = true;
            _lastCompletionResult = null;
            _lessonService.CancelLesson();
            yield return ExitToMainMenu();
        }

        private IEnumerator ExitToMainMenu()
        {
            yield return ClosePopup(_currentPopup);
            _currentPopup = null;

            SceneManager.LoadScene("MainMenu");
        }

        private IEnumerator ClosePopup(GameObject popup)
        {
            if (popup == null) yield break;

            popup.GetComponent<Ricimi.Popup>()?.Close();
            yield return new WaitForSeconds(0.55f);
            Destroy(popup);
        }

        private void SetLoading(bool visible)
        {
            if (loadingOverlay != null)
            {
                loadingOverlay.SetActive(visible);
            }
        }
    }
}
