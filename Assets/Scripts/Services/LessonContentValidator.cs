using System;
using System.Collections.Generic;
using Data.StaticData.Lesson;

namespace Services
{
    public sealed class LessonContentValidator
    {
        public List<TextQuestionDefinition> ValidateTextChoiceQuestions(TextQuestionDefinition[] questions)
        {
            if (questions == null || questions.Length == 0)
            {
                throw new Exception("Text-choice response does not contain questions.");
            }

            List<TextQuestionDefinition> result = new List<TextQuestionDefinition>(questions.Length);

            for (int i = 0; i < questions.Length; i++)
            {
                TextQuestionDefinition question = questions[i];
                if (question == null)
                {
                    throw new Exception("Text-choice question " + (i + 1) + " is null.");
                }

                RequireNotBlank(question.QuestionText, "QuestionText", i);
                ValidateAnswerSet(question.Answers, question.CorrectAnswerIndex, "text-choice", i);
                result.Add(question);
            }

            return result;
        }

        public List<ShapeQuestionDefinition> ValidateShapeQuestions(ShapeQuestionDefinition[] questions, string[] allowedShapeIds)
        {
            if (questions == null || questions.Length == 0)
            {
                throw new Exception("Shapes response does not contain questions.");
            }

            HashSet<string> allowed = new HashSet<string>(allowedShapeIds ?? Array.Empty<string>(), StringComparer.Ordinal);
            List<ShapeQuestionDefinition> result = new List<ShapeQuestionDefinition>(questions.Length);

            for (int i = 0; i < questions.Length; i++)
            {
                ShapeQuestionDefinition question = questions[i];
                if (question == null)
                {
                    throw new Exception("Shapes question " + (i + 1) + " is null.");
                }

                RequireNotBlank(question.ShapeId, "ShapeId", i);
                if (!allowed.Contains(question.ShapeId.Trim()))
                {
                    throw new Exception("Shapes question " + (i + 1) + " returned an unknown ShapeId '" + question.ShapeId + "'.");
                }

                ValidateAnswerSet(question.Answers, question.CorrectAnswerIndex, "shapes", i);
                result.Add(question);
            }

            return result;
        }

        public List<ClockQuestionDefinition> ValidateClockQuestions(ClockQuestionDefinition[] questions)
        {
            if (questions == null || questions.Length == 0)
            {
                throw new Exception("Clock response does not contain questions.");
            }

            List<ClockQuestionDefinition> result = new List<ClockQuestionDefinition>(questions.Length);

            for (int i = 0; i < questions.Length; i++)
            {
                ClockQuestionDefinition question = questions[i];
                if (question == null)
                {
                    throw new Exception("Clock question " + (i + 1) + " is null.");
                }

                if (question.Hour < 0 || question.Hour > 23)
                {
                    throw new Exception("Clock question " + (i + 1) + " has invalid Hour.");
                }

                if (question.Minute < 0 || question.Minute > 55 || question.Minute % 5 != 0)
                {
                    throw new Exception("Clock question " + (i + 1) + " has invalid Minute.");
                }

                ValidateAnswerSet(question.Answers, question.CorrectAnswerIndex, "clock", i);

                string expectedAnswer = question.Hour.ToString("00") + ":" + question.Minute.ToString("00");
                if (!string.Equals(question.Answers[question.CorrectAnswerIndex], expectedAnswer, StringComparison.Ordinal))
                {
                    throw new Exception("Clock question " + (i + 1) + " has a mismatched correct answer.");
                }

                result.Add(question);
            }

            return result;
        }

        public List<SyllableDivisionQuestionDefinition> ValidateSyllableDivisionQuestions(SyllableDivisionQuestionDefinition[] questions)
        {
            if (questions == null || questions.Length == 0)
            {
                throw new Exception("Syllable-division response does not contain questions.");
            }

            List<SyllableDivisionQuestionDefinition> result = new List<SyllableDivisionQuestionDefinition>(questions.Length);

            for (int i = 0; i < questions.Length; i++)
            {
                SyllableDivisionQuestionDefinition question = questions[i];
                if (question == null)
                {
                    throw new Exception("Syllable-division question " + (i + 1) + " is null.");
                }

                RequireNotBlank(question.PromptText, "PromptText", i);
                RequireNotBlank(question.ExpectedAnswer, "ExpectedAnswer", i);
                result.Add(question);
            }

            return result;
        }

        public List<WriteCorrectlyQuestionDefinition> ValidateWriteCorrectlyQuestions(WriteCorrectlyQuestionDefinition[] questions)
        {
            if (questions == null || questions.Length == 0)
            {
                throw new Exception("Write-correctly response does not contain questions.");
            }

            List<WriteCorrectlyQuestionDefinition> result = new List<WriteCorrectlyQuestionDefinition>(questions.Length);

            for (int i = 0; i < questions.Length; i++)
            {
                WriteCorrectlyQuestionDefinition question = questions[i];
                if (question == null)
                {
                    throw new Exception("Write-correctly question " + (i + 1) + " is null.");
                }

                RequireNotBlank(question.SentenceText, "SentenceText", i);
                RequireNotBlank(question.ExpectedAnswer, "ExpectedAnswer", i);
                result.Add(question);
            }

            return result;
        }

        public List<ReadTogetherQuestionDefinition> ValidateReadTogetherQuestions(ReadTogetherQuestionDefinition[] questions)
        {
            if (questions == null || questions.Length == 0)
            {
                throw new Exception("Read-together response does not contain questions.");
            }

            List<ReadTogetherQuestionDefinition> result = new List<ReadTogetherQuestionDefinition>(questions.Length);
            HashSet<string> seenPassages = new HashSet<string>(StringComparer.Ordinal);

            for (int i = 0; i < questions.Length; i++)
            {
                ReadTogetherQuestionDefinition question = questions[i];
                if (question == null)
                {
                    throw new Exception("Read-together question " + (i + 1) + " is null.");
                }

                RequireNotBlank(question.PassageText, "PassageText", i);
                if (!seenPassages.Add(question.PassageText.Trim()))
                {
                    throw new Exception("Read-together response contains duplicate passages.");
                }

                result.Add(question);
            }

            return result;
        }

        private static void ValidateAnswerSet(string[] answers, int correctAnswerIndex, string lessonType, int questionIndex)
        {
            if (answers == null || answers.Length != 4)
            {
                throw new Exception("Question " + (questionIndex + 1) + " in " + lessonType + " must contain exactly 4 answers.");
            }

            if (correctAnswerIndex < 0 || correctAnswerIndex > 3)
            {
                throw new Exception("Question " + (questionIndex + 1) + " in " + lessonType + " has invalid CorrectAnswerIndex.");
            }

            HashSet<string> distinctAnswers = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < answers.Length; i++)
            {
                RequireNotBlank(answers[i], "Answers[" + i + "]", questionIndex);
                if (!distinctAnswers.Add(answers[i].Trim()))
                {
                    throw new Exception("Question " + (questionIndex + 1) + " in " + lessonType + " contains duplicate answers.");
                }
            }
        }

        private static void RequireNotBlank(string value, string fieldName, int questionIndex)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new Exception("Question " + (questionIndex + 1) + " is missing " + fieldName + ".");
            }
        }
    }
}
