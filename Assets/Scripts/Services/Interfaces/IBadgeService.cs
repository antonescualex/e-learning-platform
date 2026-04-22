using System;
using System.Collections.Generic;
using Data.StaticData.Item;
using Lessons;

namespace Services.Interfaces
{
    public interface IBadgeService
    {
        event Action NotificationsAvailable;

        IReadOnlyList<ProfileItemDefinition> GetBadges();
        
        void HandleAppOpened();
        void HandleLessonCompleted(LessonCompletionResult result);
        void HandleShopPurchase();
        bool TryDequeueNotification(out BadgeDefinition badgeDefinition);
    }
}