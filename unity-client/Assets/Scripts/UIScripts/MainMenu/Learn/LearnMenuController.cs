using System.Collections;
using App;
using Enums;
using Lessons;
using Services.Interfaces;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace UIScripts.MainMenu.Learn
{
    public class LearnMenuController : MonoBehaviour
    {
        [Header("Popups")]
        [SerializeField] private GameObject subjectPopupPrefab;
        [FormerlySerializedAs("mathsPopupPrefab")] [SerializeField] private GameObject mathematicsPopupPrefab;
        [SerializeField] private GameObject englishPopupPrefab;

        [Header("Main Menu")]
        [SerializeField] private GameObject mainMenu;
        [SerializeField] private GameObject buttons;
        [SerializeField] private Canvas canvas;

        private GameObject _currentSubjectPopup;
        private GameObject _currentMathematicsPopup;
        private GameObject _currentEnglishPopup;

        public void OpenSubjectPopup() => StartCoroutine(OpenSubjectPopupDelay());
        public void OpenMathematicsPopup() => StartCoroutine(OpenMathematicsPopupDelay());
        public void OpenEnglishPopup() => StartCoroutine(OpenEnglishPopupDelay());

        public IEnumerator OpenSubjectPopupDelay()
        {
            yield return new WaitForSeconds(0.15f);
            mainMenu.SetActive(false);
            buttons.SetActive(false);

            _currentSubjectPopup = Instantiate(subjectPopupPrefab, canvas.transform);
            _currentSubjectPopup.transform.SetAsLastSibling();

            var popup = _currentSubjectPopup.GetComponent<SubjectView>();
            popup.Init(this);

            _currentSubjectPopup.GetComponent<Ricimi.Popup>()?.Open();
        }

        public IEnumerator OpenMathematicsPopupDelay()
        {
            yield return new WaitForSeconds(0.15f);

            if (_currentSubjectPopup != null)
            {
                _currentSubjectPopup.GetComponent<Ricimi.Popup>()?.Close();
                _currentSubjectPopup = null;

                yield return new WaitForSeconds(0.15f);
            }

            _currentMathematicsPopup = Instantiate(mathematicsPopupPrefab, canvas.transform);
            _currentMathematicsPopup.transform.SetAsLastSibling();

            var popup = _currentMathematicsPopup.GetComponent<MathematicsView>();
            popup.Init(this);

            _currentMathematicsPopup.GetComponent<Ricimi.Popup>()?.Open();
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

            var popup = _currentEnglishPopup.GetComponent<EnglishView>();
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

        public void CloseMathematicsPopup()
        {
            CloseSubjectPopup();

            if (_currentMathematicsPopup != null)
            {
                _currentMathematicsPopup.GetComponent<Ricimi.Popup>()?.Close();
                _currentMathematicsPopup = null;
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
            if (!ServiceContainer.TryResolve<ILessonService>(out ILessonService lessonService))
            {
                return;
            }

            lessonService.StartLesson(lessonId);
            SceneManager.LoadScene("LessonScene");
        }

    }
}
