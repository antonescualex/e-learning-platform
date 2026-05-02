using System;
using System.Collections;
using System.Collections.Generic;
using Data.StaticData.Lesson;
using Lessons;
using Services.Interfaces;
using UnityEngine;

namespace Services.OpenAI
{
    public class LessonContentServiceV2 : ILessonContentService
    {
        private readonly LessonsClient _client;
        private readonly LessonPromptBuilder _promptBuilder;
        private readonly LessonRequestFactory _requestFactory;
        private readonly LessonContentValidator _validator;

        public LessonContentServiceV2(
            string apiKeyEnvironmentVariableName = "OPENAI_API_KEY",
            string model = LessonsClient.DefaultPinnedModel,
            int timeoutSeconds = 60)
        {
            string apiKey = Environment.GetEnvironmentVariable(apiKeyEnvironmentVariableName);

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException(
                    "Missing environment variable '" + apiKeyEnvironmentVariableName + "'.");
            }

            _client = new LessonsClient(
                apiKey: apiKey.Trim(),
                model: model,
                timeoutSeconds: timeoutSeconds);

            _promptBuilder = new LessonPromptBuilder();
            _requestFactory = new LessonRequestFactory();
            _validator = new LessonContentValidator();
        }

        public IEnumerator GenerateTextChoiceLesson(
            LessonId lessonId,
            int questionCount,
            Action<List<TextQuestionDefinition>> onSuccess,
            Action<string> onError)
        {
            int normalizedCount = _requestFactory.CreateCommonRequest(lessonId, questionCount).QuestionCount;

            yield return Generate(
                _promptBuilder.BuildTextChoice(lessonId, normalizedCount),
                body => JsonUtility.FromJson<TextChoiceLessonResponse>(body),
                response => _validator.ValidateTextChoiceQuestions(response != null ? response.Questions : null),
                onSuccess,
                onError);
        }

        public IEnumerator GenerateClockLesson(
            LessonId lessonId,
            int questionCount,
            Action<List<ClockQuestionDefinition>> onSuccess,
            Action<string> onError)
        {
            int normalizedCount = _requestFactory.CreateCommonRequest(lessonId, questionCount).QuestionCount;

            yield return Generate(
                _promptBuilder.BuildClock(lessonId, normalizedCount),
                body => JsonUtility.FromJson<ClockLessonResponse>(body),
                response => _validator.ValidateClockQuestions(response != null ? response.Questions : null),
                onSuccess,
                onError);
        }

        public IEnumerator GenerateSyllableDivisionLesson(
            LessonId lessonId,
            int questionCount,
            Action<List<SyllableDivisionQuestionDefinition>> onSuccess,
            Action<string> onError)
        {
            int normalizedCount = _requestFactory.CreateCommonRequest(lessonId, questionCount).QuestionCount;

            yield return Generate(
                _promptBuilder.BuildSyllableDivision(lessonId, normalizedCount),
                body => JsonUtility.FromJson<SyllableDivisionLessonResponse>(body),
                response => _validator.ValidateSyllableDivisionQuestions(response != null ? response.Questions : null),
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
            if (!_requestFactory.TryCreateShapesRequest(
                    lessonId,
                    questionCount,
                    allowedShapeIds,
                    out LessonRequestFactory.ShapesLessonRequest request))
            {
                onError?.Invoke("Shapes lesson requires at least one valid AllowedShapeId.");
                yield break;
            }

            yield return Generate(
                _promptBuilder.BuildShapes(lessonId, request.QuestionCount, request.AllowedShapeIds),
                body => JsonUtility.FromJson<ShapesLessonResponse>(body),
                response => _validator.ValidateShapeQuestions(
                    response != null ? response.Questions : null,
                    request.AllowedShapeIds),
                onSuccess,
                onError);
        }

        public IEnumerator GenerateWriteCorrectlyLesson(
            LessonId lessonId,
            int questionCount,
            Action<List<WriteCorrectlyQuestionDefinition>> onSuccess,
            Action<string> onError)
        {
            int normalizedCount = _requestFactory.CreateCommonRequest(lessonId, questionCount).QuestionCount;

            yield return Generate(
                _promptBuilder.BuildWriteCorrectly(lessonId, normalizedCount),
                body => JsonUtility.FromJson<WriteCorrectlyLessonResponse>(body),
                response => _validator.ValidateWriteCorrectlyQuestions(response != null ? response.Questions : null),
                onSuccess,
                onError);
        }

        public IEnumerator GenerateReadTogetherLesson(
            LessonId lessonId,
            int questionCount,
            Action<List<ReadTogetherQuestionDefinition>> onSuccess,
            Action<string> onError)
        {
            int normalizedCount = _requestFactory.CreateCommonRequest(lessonId, questionCount).QuestionCount;

            yield return Generate(
                _promptBuilder.BuildReadTogether(lessonId, normalizedCount),
                body => JsonUtility.FromJson<ReadTogetherLessonResponse>(body),
                response => _validator.ValidateReadTogetherQuestions(response != null ? response.Questions : null),
                onSuccess,
                onError);
        }

        private IEnumerator Generate<TResponse, TQuestion>(
            string prompt,
            Func<string, TResponse> deserialize,
            Func<TResponse, List<TQuestion>> validate,
            Action<List<TQuestion>> onSuccess,
            Action<string> onError)
        {
            string rawJson = null;
            string requestError = null;

            yield return _client.CreateJsonResponse(
                prompt,
                content => rawJson = content,
                error => requestError = error);

            if (!string.IsNullOrWhiteSpace(requestError))
            {
                onError?.Invoke(requestError);
                yield break;
            }

            rawJson = NormalizeJson(rawJson);

            TResponse response;
            try
            {
                response = deserialize(rawJson);
            }
            catch (Exception ex)
            {
                onError?.Invoke(
                    "OpenAI returned lesson JSON that could not be deserialized.\n"
                    + ex.Message
                    + "\n\n"
                    + rawJson);
                yield break;
            }

            List<TQuestion> questions;
            try
            {
                questions = validate(response);
            }
            catch (Exception ex)
            {
                onError?.Invoke(
                    "OpenAI returned lesson JSON that failed validation.\n"
                    + ex.Message
                    + "\n\n"
                    + rawJson);
                yield break;
            }

            onSuccess?.Invoke(questions);
        }

        private static string NormalizeJson(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            value = value.Trim();

            if (value.StartsWith("```", StringComparison.Ordinal))
            {
                int firstNewLine = value.IndexOf('\n');
                int lastFence = value.LastIndexOf("```", StringComparison.Ordinal);

                if (firstNewLine >= 0 && lastFence > firstNewLine)
                {
                    value = value.Substring(firstNewLine + 1, lastFence - firstNewLine - 1).Trim();
                }
            }

            int firstBrace = value.IndexOf('{');
            int lastBrace = value.LastIndexOf('}');

            if (firstBrace >= 0 && lastBrace > firstBrace)
            {
                value = value.Substring(firstBrace, lastBrace - firstBrace + 1);
            }

            return value.Trim();
        }

        [Serializable]
        private sealed class TextChoiceLessonResponse
        {
            public TextQuestionDefinition[] Questions;
        }

        [Serializable]
        private sealed class ClockLessonResponse
        {
            public ClockQuestionDefinition[] Questions;
        }

        [Serializable]
        private sealed class ShapesLessonResponse
        {
            public ShapeQuestionDefinition[] Questions;
        }

        [Serializable]
        private sealed class SyllableDivisionLessonResponse
        {
            public SyllableDivisionQuestionDefinition[] Questions;
        }

        [Serializable]
        private sealed class WriteCorrectlyLessonResponse
        {
            public WriteCorrectlyQuestionDefinition[] Questions;
        }

        [Serializable]
        private sealed class ReadTogetherLessonResponse
        {
            public ReadTogetherQuestionDefinition[] Questions;
        }
    }
}