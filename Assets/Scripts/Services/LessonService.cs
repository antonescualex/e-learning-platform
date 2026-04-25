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
        private readonly IProfileService _profileService;
        private readonly IBadgeService _badgeService;
        private readonly IBoosterService _boosterService;

        private readonly int _baseCoinsReward = 10;
        private readonly int _baseExperienceReward = 20;
        private readonly int _coinsPerCorrectAnswer = 2;
        private readonly int _experiencePerCorrectAnswer = 4;

        private LessonId? _activeLessonId;

        public LessonService(
            IProfileService profileService,
            IBoosterService boosterService,
            IBadgeService badgeService)
        {
            _profileService = profileService;
            _boosterService = boosterService;
            _badgeService = badgeService;
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

        public bool TryCompleteLesson(int totalQuestions, int correctAnswers, float elapsedSeconds, out LessonCompletionResult result)
        {
            result = null;

            if (!_activeLessonId.HasValue)
            {
                return false;
            }

            NormalizeScore(ref totalQuestions, ref correctAnswers);

            LessonId lessonId = _activeLessonId.Value;
            result = CreateCompletionResult(lessonId, totalQuestions, correctAnswers, elapsedSeconds);

            ApplyRewards(result);
            _activeLessonId = null;
            return true;
        }

        private LessonCompletionResult CreateCompletionResult(
            LessonId lessonId,
            int totalQuestions,
            int correctAnswers,
            float elapsedSeconds)
        {
            int coinsReward = _baseCoinsReward + correctAnswers * _coinsPerCorrectAnswer;
            int experienceReward = _baseExperienceReward + correctAnswers * _experiencePerCorrectAnswer;
            
            coinsReward *= _boosterService.GetRewardMultiplier(BoosterType.DoubleCoins);
            experienceReward *= _boosterService.GetRewardMultiplier(BoosterType.DoubleXP);

            return new LessonCompletionResult(
                lessonId,
                totalQuestions,
                correctAnswers,
                Mathf.Max(0f, elapsedSeconds),
                coinsReward,
                experienceReward);
        }

        private void ApplyRewards(LessonCompletionResult result)
        {
            _profileService?.AddCoins(result.CoinsReward);
            _profileService?.AddExperience(result.ExperienceReward);
            _profileService?.RegisterCompletedLesson();

            _badgeService?.HandleLessonCompleted(result);
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
