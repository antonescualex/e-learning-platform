using System.Collections;
using Data.StaticData.Lesson;
using Lessons;
using Services;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UIScripts.Lessons
{
    public class LessonSceneController : MonoBehaviour
    {
        [Header("Popup")]
        [SerializeField] private GameObject textMathQuestionPrefab;
        [SerializeField] private GameObject imageMathQuestionPrefab;
        [SerializeField] private Canvas canvas;

        [Header("Lesson Data")]
        [SerializeField] private MathLessonContentCatalog mathLessonContentCatalog;
        [SerializeField] private int questionsPerSession = 5;

        private ILessonService _lessonService;
        private GameObject _currentPopup;
        private bool _isFinishing;

        private void Start()
        {
            if (App.Instance == null || App.Instance.LessonService == null)
            {
                SceneManager.LoadScene("Bootstrap");
                return;
            }

            _lessonService = App.Instance.LessonService;

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

            if (mathLessonContentCatalog == null)
            {
                Debug.LogWarning("LessonSceneController: lesson content catalog is not assigned.");
                yield return CancelAndExit();
                yield break;
            }

            if (TryOpenTextLesson(lessonId)) yield break;
            if (TryOpenImageLesson(lessonId)) yield break;

            Debug.LogWarning("LessonSceneController: no lesson content found for " + lessonId + ".");
            yield return CancelAndExit();
        }

        private bool TryOpenTextLesson(LessonId lessonId)
        {
            if (!mathLessonContentCatalog.TryGetRandomTextQuestionSet(lessonId, out TextMathsQuestionSet textSet))
            {
                return false;
            }

            if (textMathQuestionPrefab == null)
            {
                Debug.LogWarning("LessonSceneController: text lesson prefab is not assigned.");
                return false;
            }

            _currentPopup = Instantiate(textMathQuestionPrefab, canvas.transform);
            _currentPopup.transform.SetAsLastSibling();

            TextMathsQuestionView view = _currentPopup.GetComponent<TextMathsQuestionView>();
            if (view == null)
            {
                Debug.LogWarning("LessonSceneController: TextMathsQuestionView is missing on text lesson prefab.");
                Destroy(_currentPopup);
                _currentPopup = null;
                return false;
            }

            view.Init(
                lessonId.ToDisplayName(),
                textSet,
                Mathf.Max(1, questionsPerSession),
                OnLessonCompleted,
                OnBackRequested);

            _currentPopup.GetComponent<Ricimi.Popup>()?.Open();
            return true;
        }

        private bool TryOpenImageLesson(LessonId lessonId)
        {
            if (!mathLessonContentCatalog.TryGetRandomImageQuestionSet(lessonId, out ImageMathsQuestionSet imageSet))
            {
                return false;
            }

            if (imageMathQuestionPrefab == null)
            {
                Debug.LogWarning("LessonSceneController: image lesson prefab is not assigned.");
                return false;
            }

            _currentPopup = Instantiate(imageMathQuestionPrefab, canvas.transform);
            _currentPopup.transform.SetAsLastSibling();

            ImageMathsQuestionView view = _currentPopup.GetComponent<ImageMathsQuestionView>();
            if (view == null)
            {
                Debug.LogWarning("LessonSceneController: ImageMathsQuestionView is missing on image lesson prefab.");
                Destroy(_currentPopup);
                _currentPopup = null;
                return false;
            }

            view.Init(
                lessonId.ToDisplayName(),
                imageSet,
                Mathf.Max(1, questionsPerSession),
                OnLessonCompleted,
                OnBackRequested);

            _currentPopup.GetComponent<Ricimi.Popup>()?.Open();
            return true;
        }

        private void OnBackRequested()
        {
            if (_isFinishing) return;
            StartCoroutine(CancelAndExit());
        }

        private void OnLessonCompleted(int totalQuestions, int correctAnswers)
        {
            if (_isFinishing) return;
            StartCoroutine(CompleteAndExit(totalQuestions, correctAnswers));
        }

        private IEnumerator CompleteAndExit(int totalQuestions, int correctAnswers)
        {
            _isFinishing = true;

            if (totalQuestions <= 0)
            {
                _lessonService.CancelLesson();
                yield return ExitToMainMenu();
                yield break;
            }

            _lessonService.CompleteLesson(totalQuestions, correctAnswers);
            yield return ExitToMainMenu();
        }

        private IEnumerator CancelAndExit()
        {
            _isFinishing = true;
            _lessonService.CancelLesson();
            yield return ExitToMainMenu();
        }

        private IEnumerator ExitToMainMenu()
        {
            if (_currentPopup != null)
            {
                _currentPopup.GetComponent<Ricimi.Popup>()?.Close();
                _currentPopup = null;
                yield return new WaitForSeconds(0.55f);
            }

            SceneManager.LoadScene("MainMenu");
        }
    }
}