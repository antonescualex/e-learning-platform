using UnityEngine;

namespace UIScripts.MainMenu.Learn
{
    public class SubjectPopup : MonoBehaviour
    {
        private LearnMenuController _learnMenuController;

        public void Init(LearnMenuController learnMenuController)
        {
            _learnMenuController = learnMenuController;
        }

        public void Close()
        {
            if (_learnMenuController != null)
            {
                _learnMenuController.CloseSubjectPopup();
            }
        }

        public void OnMathsClicked()
        {
            if (_learnMenuController != null)
            {
                _learnMenuController.OpenMathsPopup();
            }
        }

        public void OnEnglishClicked()
        {
            if (_learnMenuController != null)
            {
                _learnMenuController.OpenEnglishPopup();
            }
        }
    }
}
