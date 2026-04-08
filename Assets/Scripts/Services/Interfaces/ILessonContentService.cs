using System;
using System.Collections;
using System.Collections.Generic;
using Data.StaticData.Lesson;
using Lessons;

namespace Services.Interfaces
{
    public interface ILessonContentService
    {
        IEnumerator GenerateTextChoiceLesson(
            LessonId lessonId,
            int questionCount,
            Action<List<TextQuestionDefinition>> onSuccess,
            Action<string> onError);

        IEnumerator GenerateClockLesson(
            LessonId lessonId,
            int questionCount,
            Action<List<ClockQuestionDefinition>> onSuccess,
            Action<string> onError);

        IEnumerator GenerateSyllableDivisionLesson(
            LessonId lessonId,
            int questionCount,
            Action<List<SyllableDivisionQuestionDefinition>> onSuccess,
            Action<string> onError);

        IEnumerator GenerateShapesLesson(
            LessonId lessonId,
            int questionCount,
            IReadOnlyList<string> allowedShapeIds,
            Action<List<ShapeQuestionDefinition>> onSuccess,
            Action<string> onError);

        IEnumerator GenerateWriteCorrectlyLesson(
            LessonId lessonId,
            int questionCount,
            Action<List<WriteCorrectlyQuestionDefinition>> onSuccess,
            Action<string> onError);

        IEnumerator GenerateReadTogetherLesson(
            LessonId lessonId,
            int questionCount,
            Action<List<ReadTogetherQuestionDefinition>> onSuccess,
            Action<string> onError);
    }
}
