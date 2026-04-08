using System;
using System.Collections.Generic;
using Data.StaticData.Item;
using Enums;
using Lessons;
using Services.Interfaces;
using UnityEngine;

namespace Services
{
    public class LessonService : ILessonService
    {
        private const int SpecialItemMinLessons = 10;
        private const int SpecialItemMaxLessons = 15;
        private const float BothSpecialItemsChance = 0.10f;
        private const float BoosterOnlyChanceWhenBothAvailable = 0.45f;

        private readonly IProfileService _profileService;
        private readonly BoosterCatalog _boosterCatalog;
        private readonly RewardCatalog _rewardCatalog;

        private readonly int _baseCoinsReward = 10;
        private readonly int _baseExperienceReward = 20;
        private readonly int _coinsPerCorrectAnswer = 2;
        private readonly int _experiencePerCorrectAnswer = 4;

        private LessonId? _activeLessonId;

        public LessonService(
            IProfileService profileService,
            BoosterCatalog boosterCatalog,
            RewardCatalog rewardCatalog)
        {
            _profileService = profileService;
            _boosterCatalog = boosterCatalog;
            _rewardCatalog = rewardCatalog;
        }

        public void StartLesson(LessonId lessonId)
        {
            _activeLessonId = lessonId;
        }

        public bool TryGetActiveLesson(out LessonId lessonId)
        {
            if (!_activeLessonId.HasValue)
            {
                lessonId = default;
                return false;
            }

            lessonId = _activeLessonId.Value;
            return true;
        }

        public bool TryCompleteLesson(int totalQuestions, int correctAnswers, out LessonCompletionResult result)
        {
            result = null;

            if (!_activeLessonId.HasValue)
            {
                return false;
            }

            NormalizeScore(ref totalQuestions, ref correctAnswers);

            LessonId lessonId = _activeLessonId.Value;
            result = CreateCompletionResult(lessonId, totalQuestions, correctAnswers);

            ApplyRewards(result);
            _activeLessonId = null;
            return true;
        }

        private LessonCompletionResult CreateCompletionResult(LessonId lessonId, int totalQuestions, int correctAnswers)
        {
            int coinsReward = _baseCoinsReward + correctAnswers * _coinsPerCorrectAnswer;
            int experienceReward = _baseExperienceReward + correctAnswers * _experiencePerCorrectAnswer;
            ResolveSpecialDrops(out BoosterDefinition awardedBooster, out RewardDefinition awardedReward);

            return new LessonCompletionResult(
                lessonId,
                totalQuestions,
                correctAnswers,
                coinsReward,
                experienceReward,
                awardedBooster,
                awardedReward);
        }

        private void ApplyRewards(LessonCompletionResult result)
        {
            _profileService?.AddCoins(result.CoinsReward);
            _profileService?.AddExperience(result.ExperienceReward);
            _profileService?.RegisterCompletedLesson(result.HasSpecialItems);

            if (result.AwardedBooster != null)
            {
                _profileService?.TryAddBoosterItem(result.AwardedBooster.Id);
            }

            if (result.AwardedReward != null)
            {
                _profileService?.TryAddRewardItem(result.AwardedReward.Id);
            }
        }

        private void ResolveSpecialDrops(out BoosterDefinition awardedBooster, out RewardDefinition awardedReward)
        {
            awardedBooster = null;
            awardedReward = null;

            int nextSpecialDropStreak = (_profileService?.ProfileData?.LessonsSinceLastSpecialItemDrop ?? 0) + 1;
            if (!ShouldAwardSpecialItems(nextSpecialDropStreak))
            {
                return;
            }

            List<BoosterDefinition> boosters = GetAvailableBoosters();
            List<RewardDefinition> rewards = GetAvailableRewards();
            bool hasBoosters = boosters.Count > 0;
            bool hasRewards = rewards.Count > 0;

            if (!hasBoosters && !hasRewards)
            {
                return;
            }

            if (hasBoosters && hasRewards)
            {
                float roll = UnityEngine.Random.value;
                if (roll <= BothSpecialItemsChance)
                {
                    awardedBooster = PickWeightedItem(boosters, GetBoosterWeight);
                    awardedReward = PickWeightedItem(rewards, GetRewardWeight);
                    return;
                }

                if (roll <= BothSpecialItemsChance + BoosterOnlyChanceWhenBothAvailable)
                {
                    awardedBooster = PickWeightedItem(boosters, GetBoosterWeight);
                    return;
                }

                awardedReward = PickWeightedItem(rewards, GetRewardWeight);
                return;
            }

            if (hasBoosters)
            {
                awardedBooster = PickWeightedItem(boosters, GetBoosterWeight);
                return;
            }

            awardedReward = PickWeightedItem(rewards, GetRewardWeight);
        }

        private static bool ShouldAwardSpecialItems(int nextSpecialDropStreak)
        {
            if (nextSpecialDropStreak < SpecialItemMinLessons) return false;
            if (nextSpecialDropStreak >= SpecialItemMaxLessons) return true;

            int windowSize = SpecialItemMaxLessons - SpecialItemMinLessons + 1;
            float chance = (nextSpecialDropStreak - SpecialItemMinLessons + 1) / (float)windowSize;
            return UnityEngine.Random.value <= chance;
        }

        private List<BoosterDefinition> GetAvailableBoosters()
        {
            var items = new List<BoosterDefinition>();
            IReadOnlyList<BoosterDefinition> definitions = _boosterCatalog?.BoosterDefinitions;
            if (definitions == null) return items;

            for (int i = 0; i < definitions.Count; i++)
            {
                BoosterDefinition item = definitions[i];
                if (item == null || string.IsNullOrEmpty(item.Id)) continue;

                items.Add(item);
            }

            return items;
        }

        private List<RewardDefinition> GetAvailableRewards()
        {
            var items = new List<RewardDefinition>();
            IReadOnlyList<RewardDefinition> definitions = _rewardCatalog?.RewardDefinitions;
            if (definitions == null) return items;

            for (int i = 0; i < definitions.Count; i++)
            {
                RewardDefinition item = definitions[i];
                if (item == null || string.IsNullOrEmpty(item.Id)) continue;

                items.Add(item);
            }

            return items;
        }

        private static TDefinition PickWeightedItem<TDefinition>(
            List<TDefinition> items,
            Func<TDefinition, int> weightResolver)
            where TDefinition : ProfileItemDefinition
        {
            if (items == null || items.Count == 0) return null;

            int totalWeight = 0;
            for (int i = 0; i < items.Count; i++)
            {
                totalWeight += Mathf.Max(1, weightResolver(items[i]));
            }

            int roll = UnityEngine.Random.Range(0, totalWeight);
            for (int i = 0; i < items.Count; i++)
            {
                int weight = Mathf.Max(1, weightResolver(items[i]));
                if (roll < weight)
                {
                    return items[i];
                }

                roll -= weight;
            }

            return items[items.Count - 1];
        }

        private static int GetBoosterWeight(BoosterDefinition item)
        {
            if (item == null) return 1;
            if (item.DurationSeconds >= 3600) return 25;
            if (item.DurationSeconds >= 1800) return 35;
            if (item.DurationSeconds > 0) return 65;
            return 40;
        }

        private static int GetRewardWeight(RewardDefinition item)
        {
            if (item == null) return 1;

            switch (item.RewardType)
            {
                case RewardType.Common: return 55;
                case RewardType.Rare: return 25;
                case RewardType.Epic: return 15;
                case RewardType.Legendary: return 5;
                default: return 10;
            }
        }

        private static void NormalizeScore(ref int totalQuestions, ref int correctAnswers)
        {
            if (totalQuestions < 0) totalQuestions = 0;
            if (correctAnswers < 0) correctAnswers = 0;
            if (correctAnswers > totalQuestions) correctAnswers = totalQuestions;
        }

        public void CancelLesson()
        {
            _activeLessonId = null;
        }
    }
}
