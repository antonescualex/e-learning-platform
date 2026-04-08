using Lessons;
using UnityEngine;

namespace UIScripts.MainMenu.Learn
{
    public class EnglishView : MonoBehaviour
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
                _learnMenuController.CloseEnglishPopup();
            }
        }

        public void OnReadTogetherClicked() => StartLesson(LessonId.EnglishReadTogether);
        public void OnWriteCorrectlyClicked() => StartLesson(LessonId.EnglishWriteCorrectly);
        public void OnCompleteTheSentenceClicked() => StartLesson(LessonId.EnglishCompleteTheSentence);
        public void OnSynonymsClicked() => StartLesson(LessonId.EnglishSynonyms);
        public void OnOppositesClicked() => StartLesson(LessonId.EnglishOpposites);
        public void OnSyllableDivisionClicked() => StartLesson(LessonId.EnglishSyllableDivision);

        private void StartLesson(LessonId lessonId)
        {
            if (_learnMenuController != null)
            {
                _learnMenuController.StartLesson(lessonId);
            }
        }
    }
}