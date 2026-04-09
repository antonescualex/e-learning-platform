using System.Collections.Generic;
using Data.StaticData.Item;
using Enums;
using Lessons;
using Services.Interfaces;
using UnityEngine;

namespace Services
{
    public class ProfileItemsService : IProfileItemsService
    {
        private readonly BadgeCatalog _badgeCatalog;
        private readonly BoosterCatalog _boosterCatalog;
        private readonly RewardCatalog _rewardCatalog;
        private readonly IProfileService _profileService;

        public ProfileItemsService(
            BadgeCatalog badgeCatalog,
            BoosterCatalog boosterCatalog,
            RewardCatalog rewardCatalog,
            IProfileService profileService)
        {
            _badgeCatalog = badgeCatalog;
            _boosterCatalog = boosterCatalog;
            _rewardCatalog = rewardCatalog;
            _profileService = profileService;
        }

        public IReadOnlyList<ProfileItemDefinition> GetAllItems(ProfileItemCategory profileItemCategory)
        {
            var result = new List<ProfileItemDefinition>();
            if (_profileService == null || !_profileService.HasProfile) return result;

            ProfileItemCatalogBase catalog = GetCatalog(profileItemCategory);
            if (catalog == null) return result;

            IReadOnlyList<string> ids = _profileService.ProfileData.GetItemIds(profileItemCategory);

            foreach (string id in ids)
            {
                ProfileItemDefinition definition = catalog.GetById(id);
                if (definition != null)
                {
                    result.Add(definition);
                }
            }

            return result;
        }

        public IReadOnlyList<ProfileItemDefinition> GetTopItems(ProfileItemCategory category, int count = 4)
        {
            var result = new List<ProfileItemDefinition>(count);

            if (_profileService == null || !_profileService.HasProfile) return result;

            ProfileItemCatalogBase catalog = GetCatalog(category);
            if (catalog == null) return result;

            IReadOnlyList<string> ids = _profileService.ProfileData.GetItemIds(category);
            int pageSize = Mathf.Min(count, ids.Count);

            for (int i = 0; i < pageSize; i++)
            {
                ProfileItemDefinition definition = catalog.GetById(ids[i]);
                if (definition != null)
                {
                    result.Add(definition);
                }
            }

            return result;
        }

        private ProfileItemCatalogBase GetCatalog(ProfileItemCategory category)
        {
            switch (category)
            {
                case ProfileItemCategory.Badges:
                    return _badgeCatalog;
                case ProfileItemCategory.Boosters:
                    return _boosterCatalog;
                case ProfileItemCategory.Rewards:
                    return _rewardCatalog;
                default:
                    return null;
            }
        }
    }

    public class BadgeService : IBadgeService
    {
        private const string LightningBadgeId = "badge_lightning";
        private const string SpeedRunner1BadgeId = "badge_speed_runner_1";
        private const string SpeedRunner2BadgeId = "badge_speed_runner_2";
        private const string SpeedRunner3BadgeId = "badge_speed_runner_3";
        private const string PerfectScoreBadgeId = "badge_perfect_score";

        private const string FirstStepsBadgeId = "badge_first_steps";
        private const string Learner1BadgeId = "badge_learner_1";
        private const string Learner2BadgeId = "badge_learner_2";
        private const string Learner3BadgeId = "badge_learner_3";
        private const string MasterLearnerBadgeId = "badge_master_learner";

        private const string FirstDayBadgeId = "badge_first_day";
        private const string OnFire1BadgeId = "badge_on_fire_1";
        private const string OnFire2BadgeId = "badge_on_fire_2";
        private const string AddictedBadgeId = "badge_addicted";

        private const string FirstPurchaseBadgeId = "badge_first_purchase";
        private const string CollectorBadgeId = "badge_collector";
        private const string BigSpenderBadgeId = "badge_big_spender";
        private const string ShopAddictBadgeId = "badge_shop_addict";

        private readonly BadgeCatalog _badgeCatalog;
        private readonly IProfileService _profileService;
        private readonly Queue<BadgeDefinition> _pendingNotifications = new Queue<BadgeDefinition>();

        public event System.Action NotificationsAvailable;

        public BadgeService(BadgeCatalog badgeCatalog, IProfileService profileService)
        {
            _badgeCatalog = badgeCatalog;
            _profileService = profileService;
        }

        public void HandleAppOpened()
        {
            if (!CanEvaluate()) return;

            _profileService.RegisterDailyLogin();
            EvaluateConsistencyBadges();
        }

        public void HandleLessonCompleted(LessonCompletionResult result)
        {
            if (!CanEvaluate() || result == null) return;

            EvaluateSpeedBadges(result.ElapsedSeconds);
            EvaluatePerformanceBadges(result);
            EvaluateProgressBadges();
        }

        public void HandleShopPurchase()
        {
            if (!CanEvaluate()) return;

            EvaluateShopBadges();
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

        private bool CanEvaluate()
        {
            return _badgeCatalog != null && _profileService != null && _profileService.HasProfile;
        }

        private void EvaluateConsistencyBadges()
        {
            int loginStreak = _profileService.ProfileData.CurrentLoginStreak;

            TryAwardBadge(FirstDayBadgeId, loginStreak >= 1);
            TryAwardBadge(OnFire1BadgeId, loginStreak >= 3);
            TryAwardBadge(OnFire2BadgeId, loginStreak >= 7);
            TryAwardBadge(AddictedBadgeId, loginStreak >= 30);
        }

        private void EvaluateSpeedBadges(float elapsedSeconds)
        {
            if (elapsedSeconds <= 0f) return;

            TryAwardBadge(SpeedRunner1BadgeId, elapsedSeconds < 60f);
            TryAwardBadge(SpeedRunner2BadgeId, elapsedSeconds < 40f);
            TryAwardBadge(SpeedRunner3BadgeId, elapsedSeconds < 30f);
            TryAwardBadge(LightningBadgeId, elapsedSeconds < 20f);
        }

        private void EvaluatePerformanceBadges(LessonCompletionResult result)
        {
            bool perfectScore = result.TotalQuestions > 0 && result.CorrectAnswers == result.TotalQuestions;
            TryAwardBadge(PerfectScoreBadgeId, perfectScore);
        }

        private void EvaluateProgressBadges()
        {
            int completedLessons = _profileService.ProfileData.CompletedLessonsCount;

            TryAwardBadge(FirstStepsBadgeId, completedLessons >= 1);
            TryAwardBadge(Learner1BadgeId, completedLessons >= 5);
            TryAwardBadge(Learner2BadgeId, completedLessons >= 20);
            TryAwardBadge(Learner3BadgeId, completedLessons >= 50);
            TryAwardBadge(MasterLearnerBadgeId, completedLessons >= 100);
        }

        private void EvaluateShopBadges()
        {
            int totalPurchases = _profileService.ProfileData.TotalShopPurchases;
            int totalSpentCoins = _profileService.ProfileData.TotalCoinsSpentInShop;

            TryAwardBadge(FirstPurchaseBadgeId, totalPurchases >= 1);
            TryAwardBadge(CollectorBadgeId, totalPurchases >= 5);
            TryAwardBadge(ShopAddictBadgeId, totalPurchases >= 20);
            TryAwardBadge(BigSpenderBadgeId, totalSpentCoins >= 1000);
        }

        private void TryAwardBadge(string badgeId, bool shouldAward)
        {
            if (!shouldAward || string.IsNullOrEmpty(badgeId)) return;
            BadgeDefinition badgeDefinition = _badgeCatalog.GetBadgeById(badgeId);
            if (badgeDefinition == null) return;

            if (!_profileService.TryAddBadgeItem(badgeId)) return;

            _pendingNotifications.Enqueue(badgeDefinition);
            NotificationsAvailable?.Invoke();
        }
    }
}
