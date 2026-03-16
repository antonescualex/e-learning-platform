using System.Collections;
using Lessons;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UIScripts.MainMenu.Learn
{
    public class LearnMenuController : MonoBehaviour
    {
        [Header("Popups")]
        [SerializeField] private GameObject subjectPopupPrefab;
        [SerializeField] private GameObject mathsPopupPrefab;
        [SerializeField] private GameObject englishPopupPrefab;

        [Header("Main Menu")]
        [SerializeField] private GameObject mainMenu;
        [SerializeField] private GameObject buttons;
        [SerializeField] private Canvas canvas;

        private GameObject _currentSubjectPopup;
        private GameObject _currentMathsPopup;
        private GameObject _currentEnglishPopup;

        public void OpenSubjectPopup() => StartCoroutine(OpenSubjectPopupDelay());
        public void OpenMathsPopup() => StartCoroutine(OpenMathsPopupDelay());
        public void OpenEnglishPopup() => StartCoroutine(OpenEnglishPopupDelay());

        public IEnumerator OpenSubjectPopupDelay()
        {
            yield return new WaitForSeconds(0.15f);
            mainMenu.SetActive(false);
            buttons.SetActive(false);

            _currentSubjectPopup = Instantiate(subjectPopupPrefab, canvas.transform);
            _currentSubjectPopup.transform.SetAsLastSibling();

            var popup = _currentSubjectPopup.GetComponent<SubjectPopup>();
            popup.Init(this);

            _currentSubjectPopup.GetComponent<Ricimi.Popup>()?.Open();
        }

        public IEnumerator OpenMathsPopupDelay()
        {
            yield return new WaitForSeconds(0.15f);

            if (_currentSubjectPopup != null)
            {
                _currentSubjectPopup.GetComponent<Ricimi.Popup>()?.Close();
                _currentSubjectPopup = null;

                yield return new WaitForSeconds(0.15f);
            }

            _currentMathsPopup = Instantiate(mathsPopupPrefab, canvas.transform);
            _currentMathsPopup.transform.SetAsLastSibling();

            var popup = _currentMathsPopup.GetComponent<MathsPopup>();
            popup.Init(this);

            _currentMathsPopup.GetComponent<Ricimi.Popup>()?.Open();
        }

        public IEnumerator OpenEnglishPopupDelay()
        {
            yield return new WaitForSeconds(0.15f);

            if (_currentSubjectPopup != null)
            {
                _currentSubjectPopup.GetComponent<Ricimi.Popup>()?.Close();
                _currentSubjectPopup = null;

                yield return new WaitForSeconds(0.15f);
            }

            _currentEnglishPopup = Instantiate(englishPopupPrefab, canvas.transform);
            _currentEnglishPopup.transform.SetAsLastSibling();

            var popup = _currentEnglishPopup.GetComponent<EnglishPopup>();
            popup.Init(this);

            _currentEnglishPopup.GetComponent<Ricimi.Popup>()?.Open();
        }

        public void CloseSubjectPopup()
        {
            mainMenu.SetActive(true);
            buttons.SetActive(true);

            if (_currentSubjectPopup != null)
            {
                _currentSubjectPopup.GetComponent<Ricimi.Popup>()?.Close();
                _currentSubjectPopup = null;
            }
        }

        public void CloseMathsPopup()
        {
            CloseSubjectPopup();

            if (_currentMathsPopup != null)
            {
                _currentMathsPopup.GetComponent<Ricimi.Popup>()?.Close();
                _currentMathsPopup = null;
            }
        }

        public void CloseEnglishPopup()
        {
            CloseSubjectPopup();

            if (_currentEnglishPopup != null)
            {
                _currentEnglishPopup.GetComponent<Ricimi.Popup>()?.Close();
                _currentEnglishPopup = null;
            }
        }

        public void StartLesson(LessonId lessonId)
        {
            if (App.Instance == null || App.Instance.LessonService == null) return;

            App.Instance.LessonService.StartLesson(lessonId);
            SceneManager.LoadScene("LessonScene");
        }

    }
}
