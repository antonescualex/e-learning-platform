using Enums;
using Lessons;

namespace Services
{
    public interface ILessonService
    {
        void StartLesson(LessonId lessonId);
        bool TryGetActiveLesson(out LessonId lessonId);
        void CompleteLesson(int totalQuestions, int correctAnswers);
        void CancelLesson();
    }
}