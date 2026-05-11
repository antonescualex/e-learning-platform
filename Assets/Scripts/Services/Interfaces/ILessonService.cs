using System;
using System.Collections;
using Enums;
using Lessons;

namespace Services.Interfaces
{
    public interface ILessonService
    {
        void StartLesson(LessonId lessonId);
        bool TryGetActiveLesson(out LessonId lessonId);
        IEnumerator CompleteLesson(
            int totalQuestions,
            int correctAnswers,
            float elapsedSeconds,
            Action<LessonCompletionResult> onSuccess,
            Action<string> onError);
        IEnumerator RegisterIncompleteLesson(
            float elapsedSeconds,
            Action onSuccess,
            Action<string> onError);
        void CancelLesson();
    }
}
