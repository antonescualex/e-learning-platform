using System;
using System.Collections;
using System.Collections.Generic;
using App;
using Data.StaticData.Lesson;
using Lessons;
using Services.Interfaces;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UIScripts.Lessons.Controllers
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

        private ILessonService _lessonService;
        private LessonPopupFactory _popupFactory;
        private LessonContentLoader _contentLoader;
        private LessonFlowController _flowController;

        private void Start()
        {
            if (!ServiceContainer.TryResolve<ILessonService>(out _lessonService) ||
                !ServiceContainer.TryResolve<ILessonContentService>(out ILessonContentService lessonContentService) ||
                !ServiceContainer.TryResolve<IProfileService>(out IProfileService profileService))
            {
                SceneManager.LoadScene("Bootstrap");
                return;
            }

            _popupFactory = new LessonPopupFactory(
                canvas,
                shapeSpriteCatalog,
                textQuestionPrefab,
                shapesQuestionPrefab,
                clockQuestionPrefab,
                syllableDivisionQuestionPrefab,
                readTogetherQuestionPrefab,
                writeCorrectlyQuestionPrefab,
                endLessonPrefab,
                pauseMenuPrefab);

            _contentLoader = new LessonContentLoader(
                lessonContentService,
                shapeSpriteCatalog,
                questionsPerSession,
                loadingOverlay);

            _flowController = new LessonFlowController(
                _lessonService,
                profileService,
                _popupFactory,
                routine => StartCoroutine(routine),
                sceneName => SceneManager.LoadScene(sceneName));

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
                yield return _flowController.CancelAndExit();
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
                    yield return _flowController.CancelAndExit();
                    yield break;
            }
        }

        private IEnumerator OpenTextChoiceLesson(LessonId lessonId)
        {
            yield return LoadAndOpenLesson<TextQuestionDefinition>(
                (onSuccess, onError) => _contentLoader.LoadTextChoiceLesson(lessonId, onSuccess, onError),
                questions => _popupFactory.TryOpenTextChoiceLesson(
                    lessonId,
                    questions,
                    _flowController.HandleLessonCompleted,
                    _flowController.HandleBackRequested),
                "AI text-choice lesson failed: ",
                "LessonSceneController: failed to open text-choice lesson popup.");
        }

        private IEnumerator OpenShapesLesson(LessonId lessonId)
        {
            yield return LoadAndOpenLesson<ShapeQuestionDefinition>(
                (onSuccess, onError) => _contentLoader.LoadShapesLesson(lessonId, onSuccess, onError),
                questions => _popupFactory.TryOpenShapesLesson(
                    lessonId,
                    questions,
                    _flowController.HandleLessonCompleted,
                    _flowController.HandleBackRequested),
                "AI shapes lesson failed: ",
                "LessonSceneController: failed to open shapes lesson popup.");
        }

        private IEnumerator OpenClockLesson(LessonId lessonId)
        {
            yield return LoadAndOpenLesson<ClockQuestionDefinition>(
                (onSuccess, onError) => _contentLoader.LoadClockLesson(lessonId, onSuccess, onError),
                questions => _popupFactory.TryOpenClockLesson(
                    lessonId,
                    questions,
                    _flowController.HandleLessonCompleted,
                    _flowController.HandleBackRequested),
                "AI clock lesson failed: ",
                "LessonSceneController: failed to open clock lesson popup.");
        }

        private IEnumerator OpenSyllableDivisionLesson(LessonId lessonId)
        {
            yield return LoadAndOpenLesson<SyllableDivisionQuestionDefinition>(
                (onSuccess, onError) => _contentLoader.LoadSyllableDivisionLesson(lessonId, onSuccess, onError),
                questions => _popupFactory.TryOpenSyllableDivisionLesson(
                    lessonId,
                    questions,
                    _flowController.HandleLessonCompleted,
                    _flowController.HandleBackRequested),
                "AI syllable division lesson failed: ",
                "LessonSceneController: failed to open syllable division lesson popup.");
        }

        private IEnumerator OpenReadTogetherLesson(LessonId lessonId)
        {
            yield return LoadAndOpenLesson<ReadTogetherQuestionDefinition>(
                (onSuccess, onError) => _contentLoader.LoadReadTogetherLesson(lessonId, onSuccess, onError),
                questions => _popupFactory.TryOpenReadTogetherLesson(
                    lessonId,
                    questions,
                    _flowController.HandleLessonCompleted,
                    _flowController.HandleBackRequested),
                "AI read-together lesson failed: ",
                "LessonSceneController: failed to open read-together lesson popup.");
        }

        private IEnumerator OpenWriteCorrectlyLesson(LessonId lessonId)
        {
            yield return LoadAndOpenLesson<WriteCorrectlyQuestionDefinition>(
                (onSuccess, onError) => _contentLoader.LoadWriteCorrectlyLesson(lessonId, onSuccess, onError),
                questions => _popupFactory.TryOpenWriteCorrectlyLesson(
                    lessonId,
                    questions,
                    _flowController.HandleLessonCompleted,
                    _flowController.HandleBackRequested),
                "AI write-correctly lesson failed: ",
                "LessonSceneController: failed to open write-correctly lesson popup.");
        }

        private IEnumerator LoadAndOpenLesson<TQuestion>(
            Func<Action<List<TQuestion>>, Action<string>, IEnumerator> loadLesson,
            Func<List<TQuestion>, bool> openLesson,
            string loadFailurePrefix,
            string popupFailureMessage)
        {
            List<TQuestion> generatedQuestions = null;
            string errorMessage = null;

            yield return loadLesson(
                questions => generatedQuestions = questions,
                error => errorMessage = error);

            if (generatedQuestions == null)
            {
                Debug.LogWarning(loadFailurePrefix + (string.IsNullOrWhiteSpace(errorMessage) ? "Unknown error." : errorMessage));
                yield return _flowController.CancelAndExit();
                yield break;
            }

            if (openLesson(generatedQuestions))
            {
                yield break;
            }

            Debug.LogWarning(popupFailureMessage);
            yield return _flowController.CancelAndExit();
        }
    }
}
