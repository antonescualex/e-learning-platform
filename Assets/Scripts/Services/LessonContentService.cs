using System;
using System.Collections;
using System.Collections.Generic;
using Data.StaticData.Lesson;
using Lessons;
using Services.Interfaces;
using UnityEngine;

namespace Services
{
    public class LessonContentService : ILessonContentService
    {
        public const string DefaultBaseUrl = "http://localhost:8080";
        private const int DefaultTimeoutSeconds = 45;

        private readonly LessonContentGenerator _generator;
        private readonly LessonRequestFactory _requestFactory;
        private readonly LessonContentValidator _validator;

        public LessonContentService(string baseUrl = DefaultBaseUrl, int timeoutSeconds = DefaultTimeoutSeconds)
        {
            _generator = new LessonContentGenerator(baseUrl, timeoutSeconds);
            _requestFactory = new LessonRequestFactory();
            _validator = new LessonContentValidator();
        }

        public IEnumerator GenerateTextChoiceLesson(
            LessonId lessonId,
            int questionCount,
            Action<List<TextQuestionDefinition>> onSuccess,
            Action<string> onError)
        {
            yield return _generator.Generate(
                "/text-choice",
                _requestFactory.CreateCommonRequest(lessonId, questionCount),
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
            yield return _generator.Generate(
                "/clock",
                _requestFactory.CreateCommonRequest(lessonId, questionCount),
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
            yield return _generator.Generate(
                "/syllable-division",
                _requestFactory.CreateCommonRequest(lessonId, questionCount),
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
            if (!_requestFactory.TryCreateShapesRequest(lessonId, questionCount, allowedShapeIds, out LessonRequestFactory.ShapesLessonRequest request))
            {
                onError?.Invoke("Shapes lesson requires at least one valid AllowedShapeId.");
                yield break;
            }

            yield return _generator.Generate(
                "/shapes",
                request,
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
            yield return _generator.Generate(
                "/write-correctly",
                _requestFactory.CreateCommonRequest(lessonId, questionCount),
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
            yield return _generator.Generate(
                "/read-together",
                _requestFactory.CreateCommonRequest(lessonId, questionCount),
                body => JsonUtility.FromJson<ReadTogetherLessonResponse>(body),
                response => _validator.ValidateReadTogetherQuestions(response != null ? response.Questions : null),
                onSuccess,
                onError);
        }

        [Serializable]
        private sealed class TextChoiceLessonResponse
        {
            public TextQuestionDefinition[] Questions;
        }

        [Serializable]
        private sealed class ShapesLessonResponse
        {
            public ShapeQuestionDefinition[] Questions;
        }

        [Serializable]
        private sealed class ClockLessonResponse
        {
            public ClockQuestionDefinition[] Questions;
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
