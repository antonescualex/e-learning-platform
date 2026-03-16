using System.Collections.Generic;
using Lessons;
using UnityEditor;
using UnityEngine;

namespace Data.StaticData.Lesson
{
    [CreateAssetMenu(menuName = "Lessons/Math Catalog")]
    public class MathLessonContentCatalog : ScriptableObject
    {
        [Header("Text Lesson Sets")]
        [SerializeField] private List<TextMathsQuestionSet> naturalNumbersQuestions = new List<TextMathsQuestionSet>();
        [SerializeField] private List<TextMathsQuestionSet> addSubstractQuestions = new List<TextMathsQuestionSet>();
        [SerializeField] private List<TextMathsQuestionSet> multiplyDivideQuestions = new List<TextMathsQuestionSet>();
        [SerializeField] private List<TextMathsQuestionSet> measurementsQuestions = new List<TextMathsQuestionSet>();
        [Header("Image Lesson Sets")]
        [SerializeField] private List<ImageMathsQuestionSet> geometricalShapesQuestions = new List<ImageMathsQuestionSet>();
        [SerializeField] private List<ImageMathsQuestionSet> whatTimeQuestions = new List<ImageMathsQuestionSet>();

        public bool TryGetRandomTextQuestionSet(LessonId lessonId, out TextMathsQuestionSet questionSet)
        {
            switch (lessonId)
            {
                case LessonId.MathematicsNaturalNumbers:
                    return TryGetRandomFromList(naturalNumbersQuestions, out questionSet);
                case LessonId.MathematicsAddAndSubtract:
                    return TryGetRandomFromList(addSubstractQuestions, out questionSet);
                case LessonId.MathematicsMultiplyAndDivide:
                    return TryGetRandomFromList(multiplyDivideQuestions, out questionSet);
                case LessonId.MathematicsMeasurements:
                    return TryGetRandomFromList(measurementsQuestions, out questionSet);
                default:
                    questionSet = null;
                    return false;
            }
        }

        public bool TryGetRandomImageQuestionSet(LessonId lessonId, out ImageMathsQuestionSet questionSet)
        {
            switch (lessonId)
            {
                case LessonId.MathematicsGeometricalShapes:
                    return TryGetRandomFromList(geometricalShapesQuestions, out questionSet);
                case LessonId.MathematicsWhatIsTheTime:
                    return TryGetRandomFromList(whatTimeQuestions, out questionSet);
                default:
                    questionSet = null;
                    return false;
            }
        }

        private static bool TryGetRandomFromList<T>(List<T> list, out T result) where T : ScriptableObject
        {
            result = null;
            if (list == null || list.Count == 0) return false;

            List<T> valid = new List<T>();
            foreach (T item in list)
            {
                if (item != null) valid.Add(item);
            }

            if (valid.Count == 0) return false;

            result = valid[Random.Range(0, valid.Count)];
            return true;
        }
    }
}