using Enums;
using Lessons;

namespace Services.Interfaces
{
    public interface ILessonService
    {
        void StartLesson(LessonId lessonId);
        bool TryGetActiveLesson(out LessonId lessonId);
        bool TryCompleteLesson(int totalQuestions, int correctAnswers, float elapsedSeconds, out LessonCompletionResult result);
        void CancelLesson();
    }
}
