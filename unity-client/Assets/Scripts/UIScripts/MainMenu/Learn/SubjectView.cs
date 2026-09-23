using UnityEngine;

namespace UIScripts.MainMenu.Learn
{
    public class SubjectView : MonoBehaviour
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

        public void OnMathematicsClicked()
        {
            if (_learnMenuController != null)
            {
                _learnMenuController.OpenMathematicsPopup();
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
