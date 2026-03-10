using UnityEngine;

namespace UIScripts.MainMenu.Learn
{
    public class MathsPopup : MonoBehaviour
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
                _learnMenuController.CloseMathsPopup();
            }
        }
    }
}