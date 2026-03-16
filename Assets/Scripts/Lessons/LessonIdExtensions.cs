namespace Lessons
{
    public static class LessonIdExtensions
    {
        public static LessonSubject GetSubject(this LessonId lessonId)
        {
            switch (lessonId)
            {
                case LessonId.MathematicsNaturalNumbers:
                case LessonId.MathematicsGeometricalShapes:
                case LessonId.MathematicsAddAndSubtract:
                case LessonId.MathematicsWhatIsTheTime:
                case LessonId.MathematicsMeasurements:
                case LessonId.MathematicsMultiplyAndDivide:
                    return LessonSubject.Mathematics;
                default:
                    return LessonSubject.English;
            }
        }

        public static string ToDisplayName(this LessonId lessonId)
        {
            switch (lessonId)
            {
                case LessonId.MathematicsNaturalNumbers: return "Natural numbers";
                case LessonId.MathematicsGeometricalShapes: return "Geometrical shapes";
                case LessonId.MathematicsAddAndSubtract: return "Add & Subtract";
                case LessonId.MathematicsWhatIsTheTime: return "What is the time?";
                case LessonId.MathematicsMeasurements: return "Measurements";
                case LessonId.MathematicsMultiplyAndDivide: return "Multiply & Divide";
                case LessonId.EnglishReadTogether: return "Read together";
                case LessonId.EnglishWriteCorrectly: return "Write correctly";
                case LessonId.EnglishCompleteTheSentence: return "Complete the sentence";
                case LessonId.EnglishSynonyms: return "Synonyms";
                case LessonId.EnglishOpposites: return "Opposites";
                case LessonId.EnglishSyllableDivision: return "Syllable division";
                default: return lessonId.ToString();
            }
        }

        public static string ToDisplayName(this LessonSubject subject)
        {
            return subject == LessonSubject.Mathematics ? "Mathematics" : "English";
        }
    }
}