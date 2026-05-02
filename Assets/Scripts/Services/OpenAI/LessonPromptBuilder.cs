using System;
using System.Text;
using Lessons;

namespace Services.OpenAI
{
    public class LessonPromptBuilder
    {
       public string BuildTextChoice(LessonId lessonId, int questionCount)
        {
            return BuildPrompt(
                lessonId,
                questionCount,
                "Create multiple-choice questions about " + DescribeTextChoiceFocus(lessonId) + ".",
                "{\"Questions\":[{\"QuestionText\":\"string\",\"Answers\":[\"string\",\"string\",\"string\",\"string\"],\"CorrectAnswerIndex\":0}]}",
                "Return exactly " + questionCount + " questions.",
                "Answers must contain exactly 4 distinct values.",
                "CorrectAnswerIndex must be between 0 and 3.",
                "Questions must be short, clear, and child-friendly.");
        }

        public string BuildClock(LessonId lessonId, int questionCount)
        {
            return BuildPrompt(
                lessonId,
                questionCount,
                "Create clock-reading questions.",
                "{\"Questions\":[{\"Hour\":8,\"Minute\":30,\"Answers\":[\"08:30\",\"08:35\",\"09:30\",\"07:30\"],\"CorrectAnswerIndex\":0}]}",
                "Return exactly " + questionCount + " questions.",
                "Hour must be between 0 and 23.",
                "Minute must be a multiple of 5.",
                "Answers must contain exactly 4 distinct HH:mm values with leading zeroes.",
                "Answers[CorrectAnswerIndex] must match Hour and Minute exactly.");
        }

        public string BuildShapes(LessonId lessonId, int questionCount, string[] allowedShapeIds)
        {
            return BuildPrompt(
                lessonId,
                questionCount,
                "Create shape-identification questions.",
                "{\"Questions\":[{\"ShapeId\":\"circle\",\"Answers\":[\"Circle\",\"Square\",\"Triangle\",\"Rectangle\"],\"CorrectAnswerIndex\":0}]}",
                "Return exactly " + questionCount + " questions.",
                "ShapeId must be one of these exact values: " + string.Join(", ", allowedShapeIds ?? Array.Empty<string>()),
                "Answers must contain exactly 4 distinct English shape names.",
                "The correct answer text must match ShapeId.",
                "Use only allowed ShapeIds.");
        }

        public string BuildSyllableDivision(LessonId lessonId, int questionCount)
        {
            return BuildPrompt(
                lessonId,
                questionCount,
                "Create syllable-division exercises.",
                "{\"Questions\":[{\"PromptText\":\"banana\",\"ExpectedAnswer\":\"ba-na-na\"}]}",
                "Return exactly " + questionCount + " questions.",
                "PromptText must contain only the word to split.",
                "ExpectedAnswer must be the same word split with hyphens.",
                "Use simple child-friendly English words.");
        }

        public string BuildWriteCorrectly(LessonId lessonId, int questionCount)
        {
            return BuildPrompt(
                lessonId,
                questionCount,
                "Create dictation-style exercises.",
                "{\"Questions\":[{\"SentenceText\":\"The cat is on the mat.\",\"ExpectedAnswer\":\"The cat is on the mat.\"}]}",
                "Return exactly " + questionCount + " questions.",
                "Use short, clear, easy-to-hear English sentences.",
                "ExpectedAnswer must be exactly identical to SentenceText.",
                "Avoid abbreviations and unusual punctuation.");
        }

        public string BuildReadTogether(LessonId lessonId, int questionCount)
        {
            return BuildPrompt(
                lessonId,
                questionCount,
                "Create short passages for reading aloud.",
                "{\"Questions\":[{\"PassageText\":\"The sun is bright today.\"}]}",
                "Return exactly " + questionCount + " questions.",
                "Each passage must be short and easy for a child to read aloud.",
                "Use common words.",
                "Avoid duplicate passages.");
        }

        private static string BuildPrompt(
            LessonId lessonId,
            int questionCount,
            string task,
            string jsonExample,
            params string[] rules)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("Generate lesson content as a single JSON object.");
            builder.AppendLine("Return only valid JSON.");
            builder.AppendLine("Do not return markdown.");
            builder.AppendLine("Do not return code fences.");
            builder.AppendLine("Do not return explanations.");
            builder.AppendLine("Do not add extra fields.");
            builder.AppendLine("All visible lesson content must be in English.");
            builder.AppendLine("Content must be child-safe and age-appropriate for ages 6 to 10.");
            builder.AppendLine("LessonId: " + lessonId);
            builder.AppendLine("LessonName: " + lessonId.ToDisplayName());
            builder.AppendLine("QuestionCount: " + questionCount);
            builder.AppendLine("Task: " + task);
            builder.AppendLine("Return JSON in exactly this shape:");
            builder.AppendLine(jsonExample);
            builder.AppendLine("Rules:");

            for (int i = 0; i < rules.Length; i++)
            {
                builder.Append("- ").AppendLine(rules[i]);
            }

            return builder.ToString().TrimEnd();
        }

        private static string DescribeTextChoiceFocus(LessonId lessonId)
        {
            switch (lessonId)
            {
                case LessonId.MathematicsNaturalNumbers:
                    return "counting, comparing, and ordering natural numbers";
                case LessonId.MathematicsAddAndSubtract:
                    return "simple addition and subtraction";
                case LessonId.MathematicsMeasurements:
                    return "basic measurements and simple comparisons";
                case LessonId.MathematicsMultiplyAndDivide:
                    return "simple multiplication and division";
                case LessonId.EnglishCompleteTheSentence:
                    return "choosing the correct word to complete a sentence";
                case LessonId.EnglishSynonyms:
                    return "choosing simple synonyms";
                case LessonId.EnglishOpposites:
                    return "choosing simple opposites";
                default:
                    return "child-friendly multiple-choice practice";
            }
        }
    }
}