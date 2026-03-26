using Data.StaticData.Lesson;

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

        public static LessonViewType GetViewType(this LessonId lessonId)
        {
            switch (lessonId)
            {
                case LessonId.MathematicsNaturalNumbers:
                case LessonId.MathematicsAddAndSubtract:
                case LessonId.MathematicsMeasurements:
                case LessonId.MathematicsMultiplyAndDivide:
                case LessonId.EnglishCompleteTheSentence:
                case LessonId.EnglishSynonyms:
                case LessonId.EnglishOpposites:
                    return LessonViewType.TextChoice;

                case LessonId.MathematicsGeometricalShapes:
                    return LessonViewType.GeometricalShapes;

                case LessonId.MathematicsWhatIsTheTime:
                    return LessonViewType.Clock;

                case LessonId.EnglishSyllableDivision:
                    return LessonViewType.SyllableDivision;

                case LessonId.EnglishReadTogether:
                    return LessonViewType.ReadTogether;

                case LessonId.EnglishWriteCorrectly:
                    return LessonViewType.WriteCorrectly;

                default:
                    return LessonViewType.TextChoice;
            }
        }
    }
}