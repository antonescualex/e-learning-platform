using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Data.StaticData.Lesson;
using Lessons;
using Services.Interfaces;
using UnityEngine;
using UnityEngine.Networking;

namespace Services
{
    public class LessonContentService : ILessonContentService
    {
        public const string DefaultBaseUrl = "http://localhost:8080";
        private const string LessonContentPath = "/api/lesson-content";
        private const int DefaultTimeoutSeconds = 45;

        private static readonly Regex AllowedShapeIdRegex =
            new Regex("^[A-Za-z0-9_-]{1,40}$", RegexOptions.Compiled);

        private readonly string _baseUrl;
        private readonly int _timeoutSeconds;

        public LessonContentService(string baseUrl = DefaultBaseUrl, int timeoutSeconds = DefaultTimeoutSeconds)
        {
            _baseUrl = string.IsNullOrWhiteSpace(baseUrl) ? DefaultBaseUrl : baseUrl.Trim().TrimEnd('/');
            _timeoutSeconds = Mathf.Max(1, timeoutSeconds);
        }

        public IEnumerator GenerateTextChoiceLesson(
            LessonId lessonId,
            int questionCount,
            Action<List<TextMathsQuestionDefinition>> onSuccess,
            Action<string> onError)
        {
            yield return SendRequest(
                "/text-choice",
                CreateCommonRequest(lessonId, questionCount),
                body => JsonUtility.FromJson<TextChoiceLessonResponse>(body),
                response => ValidateTextChoiceQuestions(response != null ? response.Questions : null),
                onSuccess,
                onError);
        }

        public IEnumerator GenerateClockLesson(
            LessonId lessonId,
            int questionCount,
            Action<List<ClockQuestionDefinition>> onSuccess,
            Action<string> onError)
        {
            yield return SendRequest(
                "/clock",
                CreateCommonRequest(lessonId, questionCount),
                body => JsonUtility.FromJson<ClockLessonResponse>(body),
                response => ValidateClockQuestions(response != null ? response.Questions : null),
                onSuccess,
                onError);
        }

        public IEnumerator GenerateSyllableDivisionLesson(
            LessonId lessonId,
            int questionCount,
            Action<List<SyllableDivisionQuestionDefinition>> onSuccess,
            Action<string> onError)
        {
            yield return SendRequest(
                "/syllable-division",
                CreateCommonRequest(lessonId, questionCount),
                body => JsonUtility.FromJson<SyllableDivisionLessonResponse>(body),
                response => ValidateSyllableDivisionQuestions(response != null ? response.Questions : null),
                onSuccess,
                onError);
        }

        public IEnumerator GenerateShapesLesson(
            LessonId lessonId,
            int questionCount,
            IReadOnlyList<string> allowedShapeIds,
            Action<List<ShapeQuestionDefinition>> onSuccess,
            Action<string> onError)
        {
            string[] normalizedShapeIds = NormalizeAllowedShapeIds(allowedShapeIds);
            if (normalizedShapeIds.Length == 0)
            {
                onError?.Invoke("Shapes lesson requires at least one valid AllowedShapeId.");
                yield break;
            }

            yield return SendRequest(
                "/shapes",
                CreateShapesRequest(lessonId, questionCount, normalizedShapeIds),
                body => JsonUtility.FromJson<ShapesLessonResponse>(body),
                response => ValidateShapeQuestions(response != null ? response.Questions : null, normalizedShapeIds),
                onSuccess,
                onError);
        }

        public IEnumerator GenerateWriteCorrectlyLesson(
            LessonId lessonId,
            int questionCount,
            Action<List<WriteCorrectlyQuestionDefinition>> onSuccess,
            Action<string> onError)
        {
            yield return SendRequest(
                "/write-correctly",
                CreateCommonRequest(lessonId, questionCount),
                body => JsonUtility.FromJson<WriteCorrectlyLessonResponse>(body),
                response => ValidateWriteCorrectlyQuestions(response != null ? response.Questions : null),
                onSuccess,
                onError);
        }

        public IEnumerator GenerateReadTogetherLesson(
            LessonId lessonId,
            int questionCount,
            Action<List<ReadTogetherQuestionDefinition>> onSuccess,
            Action<string> onError)
        {
            yield return SendRequest(
                "/read-together",
                CreateCommonRequest(lessonId, questionCount),
                body => JsonUtility.FromJson<ReadTogetherLessonResponse>(body),
                response => ValidateReadTogetherQuestions(response != null ? response.Questions : null),
                onSuccess,
                onError);
        }

        private IEnumerator SendRequest<TRequest, TResponse, TQuestion>(
            string endpointPath,
            TRequest payload,
            Func<string, TResponse> deserialize,
            Func<TResponse, List<TQuestion>> validate,
            Action<List<TQuestion>> onSuccess,
            Action<string> onError)
        {
            string requestJson = JsonUtility.ToJson(payload);
            byte[] requestBody = Encoding.UTF8.GetBytes(requestJson);

            using UnityWebRequest request = new UnityWebRequest(BuildUrl(endpointPath), UnityWebRequest.kHttpVerbPOST);
            request.uploadHandler = new UploadHandlerRaw(requestBody);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.timeout = _timeoutSeconds;
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            string responseText = request.downloadHandler != null ? request.downloadHandler.text : string.Empty;

            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke(FormatRequestError(request, responseText));
                yield break;
            }

            TResponse response;
            try
            {
                response = deserialize(responseText);
            }
            catch (Exception ex)
            {
                onError?.Invoke("Invalid JSON response from lesson backend.\n" + ex.Message);
                yield break;
            }

            List<TQuestion> questions;
            try
            {
                questions = validate(response);
            }
            catch (Exception ex)
            {
                onError?.Invoke("Lesson backend returned invalid content.\n" + ex.Message);
                yield break;
            }

            onSuccess?.Invoke(questions);
        }

        private CommonLessonRequest CreateCommonRequest(LessonId lessonId, int questionCount)
        {
            return new CommonLessonRequest
            {
                LessonId = lessonId.ToString(),
                QuestionCount = NormalizeQuestionCount(questionCount)
            };
        }

        private ShapesLessonRequest CreateShapesRequest(LessonId lessonId, int questionCount, string[] allowedShapeIds)
        {
            return new ShapesLessonRequest
            {
                LessonId = lessonId.ToString(),
                QuestionCount = NormalizeQuestionCount(questionCount),
                AllowedShapeIds = allowedShapeIds
            };
        }

        private static int NormalizeQuestionCount(int questionCount)
        {
            return Mathf.Clamp(questionCount, 1, 10);
        }

        private string BuildUrl(string endpointPath)
        {
            return _baseUrl + LessonContentPath + endpointPath;
        }

        private static string[] NormalizeAllowedShapeIds(IReadOnlyList<string> allowedShapeIds)
        {
            List<string> result = new List<string>();
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);

            if (allowedShapeIds == null)
            {
                return result.ToArray();
            }

            for (int i = 0; i < allowedShapeIds.Count; i++)
            {
                string candidate = allowedShapeIds[i];
                if (string.IsNullOrWhiteSpace(candidate))
                {
                    continue;
                }

                candidate = candidate.Trim();
                if (!AllowedShapeIdRegex.IsMatch(candidate))
                {
                    continue;
                }

                if (seen.Add(candidate))
                {
                    result.Add(candidate);
                }
            }

            return result.ToArray();
        }

        private static string FormatRequestError(UnityWebRequest request, string responseText)
        {
            ApiErrorResponse error = null;

            if (!string.IsNullOrWhiteSpace(responseText))
            {
                try
                {
                    error = JsonUtility.FromJson<ApiErrorResponse>(responseText);
                }
                catch
                {
                    error = null;
                }
            }

            if (error != null && !string.IsNullOrWhiteSpace(error.Message))
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(error.Code).Append(": ").Append(error.Message);

                if (error.Details != null && error.Details.Length > 0)
                {
                    builder.Append("\n").Append(string.Join("\n", error.Details));
                }

                if (!string.IsNullOrWhiteSpace(error.CorrelationId))
                {
                    builder.Append("\nCorrelationId: ").Append(error.CorrelationId);
                }

                return builder.ToString();
            }

            if (!string.IsNullOrWhiteSpace(responseText))
            {
                return "Lesson backend request failed: " + request.error + "\n" + responseText;
            }

            return "Lesson backend request failed: " + request.error;
        }

        private static List<TextMathsQuestionDefinition> ValidateTextChoiceQuestions(TextMathsQuestionDefinition[] questions)
        {
            if (questions == null || questions.Length == 0)
            {
                throw new Exception("Text-choice response does not contain questions.");
            }

            List<TextMathsQuestionDefinition> result = new List<TextMathsQuestionDefinition>(questions.Length);

            for (int i = 0; i < questions.Length; i++)
            {
                TextMathsQuestionDefinition question = questions[i];
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

        private static List<ShapeQuestionDefinition> ValidateShapeQuestions(ShapeQuestionDefinition[] questions, string[] allowedShapeIds)
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

        private static List<ClockQuestionDefinition> ValidateClockQuestions(ClockQuestionDefinition[] questions)
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

        private static List<SyllableDivisionQuestionDefinition> ValidateSyllableDivisionQuestions(SyllableDivisionQuestionDefinition[] questions)
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

        private static List<WriteCorrectlyQuestionDefinition> ValidateWriteCorrectlyQuestions(WriteCorrectlyQuestionDefinition[] questions)
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

        private static List<ReadTogetherQuestionDefinition> ValidateReadTogetherQuestions(ReadTogetherQuestionDefinition[] questions)
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

        [Serializable]
        private class CommonLessonRequest
        {
            public string LessonId;
            public int QuestionCount;
        }

        [Serializable]
        private class ShapesLessonRequest
        {
            public string LessonId;
            public int QuestionCount;
            public string[] AllowedShapeIds;
        }

        [Serializable]
        private class TextChoiceLessonResponse
        {
            public TextMathsQuestionDefinition[] Questions;
        }

        [Serializable]
        private class ShapesLessonResponse
        {
            public ShapeQuestionDefinition[] Questions;
        }

        [Serializable]
        private class ClockLessonResponse
        {
            public ClockQuestionDefinition[] Questions;
        }

        [Serializable]
        private class SyllableDivisionLessonResponse
        {
            public SyllableDivisionQuestionDefinition[] Questions;
        }

        [Serializable]
        private class WriteCorrectlyLessonResponse
        {
            public WriteCorrectlyQuestionDefinition[] Questions;
        }

        [Serializable]
        private class ReadTogetherLessonResponse
        {
            public ReadTogetherQuestionDefinition[] Questions;
        }

        [Serializable]
        private class ApiErrorResponse
        {
            public string Code;
            public string Message;
            public string[] Details;
            public string CorrelationId;
        }
    }
}
