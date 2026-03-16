using Lessons;
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

        public void OnNaturalNumbersClicked() => StartLesson(LessonId.MathematicsNaturalNumbers);
        public void OnGeometricalShapesClicked() => StartLesson(LessonId.MathematicsGeometricalShapes);
        public void OnAddAndSubtractClicked() => StartLesson(LessonId.MathematicsAddAndSubtract);
        public void OnWhatIsTheTimeClicked() => StartLesson(LessonId.MathematicsWhatIsTheTime);
        public void OnMeasurementsClicked() => StartLesson(LessonId.MathematicsMeasurements);
        public void OnMultiplyAndDivideClicked() => StartLesson(LessonId.MathematicsMultiplyAndDivide);

        private void StartLesson(LessonId lessonId)
        {
            if (_learnMenuController != null)
            {
                _learnMenuController.StartLesson(lessonId);
            }
        }
    }
}