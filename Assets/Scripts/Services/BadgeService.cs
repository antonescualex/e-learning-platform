using System;
using System.Collections.Generic;
using Data.StaticData.Item;
using Enums;
using Lessons;
using Services.Interfaces;

namespace Services
{
    public class BadgeService : IBadgeService
    {
        private readonly BadgeCatalog _badgeCatalog;
        private readonly IProfileService _profileService;
        private readonly Queue<BadgeDefinition> _pendingNotifications = new Queue<BadgeDefinition>();

        public event Action NotificationsAvailable;

        public BadgeService(BadgeCatalog badgeCatalog, IProfileService profileService)
        {
            _badgeCatalog = badgeCatalog;
            _profileService = profileService;
        }

        public IReadOnlyList<ProfileItemDefinition> GetBadges()
        {
            var result = new List<ProfileItemDefinition>();
            if (_profileService == null || !_profileService.HasProfile) return result;
            if (_badgeCatalog == null) return result;

            IReadOnlyList<string> ids = _profileService.ProfileData.GetItemIds(ProfileItemCategory.Badges);

            foreach (string id in ids)
            {
                ProfileItemDefinition definition = _badgeCatalog.GetById(id);
                if (definition != null)
                {
                    result.Add(definition);
                }
            }

            return result;
        }
        
        public void EnqueueAwardedBadges(IEnumerable<string> badgeIds)
        {
            if (badgeIds == null || _badgeCatalog == null) return;

            bool hasNotifications = false;

            foreach (string badgeId in badgeIds)
            {
                if (string.IsNullOrWhiteSpace(badgeId)) continue;

                BadgeDefinition badgeDefinition = _badgeCatalog.GetBadgeById(badgeId);
                if (badgeDefinition == null) continue;

                _pendingNotifications.Enqueue(badgeDefinition);
                hasNotifications = true;
            }

            if (hasNotifications)
            {
                NotificationsAvailable?.Invoke();
            }
        }

        public bool TryDequeueNotification(out BadgeDefinition badgeDefinition)
        {
            if (_pendingNotifications.Count == 0)
            {
                badgeDefinition = null;
                return false;
            }

            badgeDefinition = _pendingNotifications.Dequeue();
            return badgeDefinition != null;
        }
    }
}