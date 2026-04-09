using System;
using Data.StaticData.Item;
using Enums;
using Lessons;

namespace Services.Interfaces
{
    public interface IBadgeService
    {
        event Action NotificationsAvailable;

        void HandleAppOpened();
        void HandleLessonCompleted(LessonCompletionResult result);
        void HandleShopPurchase();
        bool TryDequeueNotification(out BadgeDefinition badgeDefinition);
    }

    public interface ILessonService
    {
        void StartLesson(LessonId lessonId);
        bool TryGetActiveLesson(out LessonId lessonId);
        bool TryCompleteLesson(int totalQuestions, int correctAnswers, float elapsedSeconds, out LessonCompletionResult result);
        void CancelLesson();
    }
}
