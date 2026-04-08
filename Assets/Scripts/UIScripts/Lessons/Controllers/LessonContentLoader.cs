using System;
using System.Collections;
using System.Collections.Generic;
using Data.StaticData.Lesson;
using Lessons;
using Services.Interfaces;
using UnityEngine;

namespace UIScripts.Lessons.Controllers
{
    public sealed class LessonContentLoader
    {
        private readonly ILessonContentService _lessonContentService;
        private readonly ShapeSpriteCatalog _shapeSpriteCatalog;
        private readonly int _questionsPerSession;
        private readonly GameObject _loadingOverlay;

        public LessonContentLoader(
            ILessonContentService lessonContentService,
            ShapeSpriteCatalog shapeSpriteCatalog,
            int questionsPerSession,
            GameObject loadingOverlay)
        {
            _lessonContentService = lessonContentService;
            _shapeSpriteCatalog = shapeSpriteCatalog;
            _questionsPerSession = questionsPerSession;
            _loadingOverlay = loadingOverlay;
        }

        public IEnumerator LoadTextChoiceLesson(
            LessonId lessonId,
            Action<List<TextQuestionDefinition>> onSuccess,
            Action<string> onError)
        {
            if (_lessonContentService == null)
            {
                onError?.Invoke("Text-choice lesson requires ILessonContentService.");
                yield break;
            }

            yield return ExecuteWithLoading(_lessonContentService.GenerateTextChoiceLesson(
                lessonId,
                GetQuestionCount(),
                onSuccess,
                onError));
        }

        public IEnumerator LoadShapesLesson(
            LessonId lessonId,
            Action<List<ShapeQuestionDefinition>> onSuccess,
            Action<string> onError)
        {
            IReadOnlyList<string> allowedShapeIds = _shapeSpriteCatalog != null
                ? _shapeSpriteCatalog.GetAllIds()
                : null;

            if (_lessonContentService == null || allowedShapeIds == null || allowedShapeIds.Count == 0)
            {
                onError?.Invoke("Shapes lesson requires ILessonContentService and ShapeSpriteCatalog.");
                yield break;
            }

            yield return ExecuteWithLoading(_lessonContentService.GenerateShapesLesson(
                lessonId,
                GetQuestionCount(),
                allowedShapeIds,
                onSuccess,
                onError));
        }

        public IEnumerator LoadClockLesson(
            LessonId lessonId,
            Action<List<ClockQuestionDefinition>> onSuccess,
            Action<string> onError)
        {
            if (_lessonContentService == null)
            {
                onError?.Invoke("Clock lesson requires ILessonContentService.");
                yield break;
            }

            yield return ExecuteWithLoading(_lessonContentService.GenerateClockLesson(
                lessonId,
                GetQuestionCount(),
                onSuccess,
                onError));
        }

        public IEnumerator LoadSyllableDivisionLesson(
            LessonId lessonId,
            Action<List<SyllableDivisionQuestionDefinition>> onSuccess,
            Action<string> onError)
        {
            if (_lessonContentService == null)
            {
                onError?.Invoke("Syllable division lesson requires ILessonContentService.");
                yield break;
            }

            yield return ExecuteWithLoading(_lessonContentService.GenerateSyllableDivisionLesson(
                lessonId,
                GetQuestionCount(),
                onSuccess,
                onError));
        }

        public IEnumerator LoadReadTogetherLesson(
            LessonId lessonId,
            Action<List<ReadTogetherQuestionDefinition>> onSuccess,
            Action<string> onError)
        {
            if (_lessonContentService == null)
            {
                onError?.Invoke("ReadTogether lesson requires ILessonContentService.");
                yield break;
            }

            yield return ExecuteWithLoading(_lessonContentService.GenerateReadTogetherLesson(
                lessonId,
                GetQuestionCount(),
                onSuccess,
                onError));
        }

        public IEnumerator LoadWriteCorrectlyLesson(
            LessonId lessonId,
            Action<List<WriteCorrectlyQuestionDefinition>> onSuccess,
            Action<string> onError)
        {
            if (_lessonContentService == null)
            {
                onError?.Invoke("WriteCorrectly lesson requires ILessonContentService.");
                yield break;
            }

            yield return ExecuteWithLoading(_lessonContentService.GenerateWriteCorrectlyLesson(
                lessonId,
                GetQuestionCount(),
                onSuccess,
                onError));
        }

        private int GetQuestionCount()
        {
            return Mathf.Max(1, _questionsPerSession);
        }

        private IEnumerator ExecuteWithLoading(IEnumerator operation)
        {
            SetLoading(true);
            yield return operation;
            SetLoading(false);
        }

        private void SetLoading(bool visible)
        {
            if (_loadingOverlay != null)
            {
                _loadingOverlay.SetActive(visible);
            }
        }
    }
}
