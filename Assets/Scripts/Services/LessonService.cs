using System;
using System.Collections;
using Clients;
using Clients.Interfaces;
using Dto.Lesson;
using Dto.Profile;
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
        private readonly ILessonClient _lessonClient;

        private readonly int _baseCoinsReward = 10;
        private readonly int _baseExperienceReward = 20;
        private readonly int _coinsPerCorrectAnswer = 2;
        private readonly int _experiencePerCorrectAnswer = 4;

        private LessonId? _activeLessonId;

        public LessonService(
            IProfileService profileService,
            IBoosterService boosterService,
            IBadgeService badgeService,
            ILessonClient lessonClient)
        {
            _profileService = profileService;
            _boosterService = boosterService;
            _badgeService = badgeService;
            _lessonClient = lessonClient;
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

        public IEnumerator CompleteLesson(int totalQuestions, int correctAnswers, float elapsedSeconds, Action<LessonCompletionResult> onSuccess, Action<string> onError)
        {
            if (!_activeLessonId.HasValue)
            {
                onError?.Invoke("No active lesson.");
                yield break;
            }

            NormalizeScore(ref totalQuestions, ref correctAnswers);

            LessonId lessonId = _activeLessonId.Value;
            LessonCompletionResult result = CreateCompletionResult(lessonId, totalQuestions, correctAnswers, elapsedSeconds);

            var request = new CompleteLessonRequest
            {
                LessonId = lessonId.ToString(),
                Completed = true,
                CorrectAnswersCount = correctAnswers,
                QuestionCount = totalQuestions,
                AwardedExperience = result.ExperienceReward,
                AwardedCoins = result.CoinsReward,
                ElapsedSeconds = result.ElapsedSeconds
            };

            ProfileAwardResponse response = null;
            string error = null;

            yield return _lessonClient.CompleteLesson(request, r => response = r, e => error = e);

            if (!string.IsNullOrWhiteSpace(error) || response == null || response.Profile == null)
            {
                onError?.Invoke(error);
                yield break;
            }

            _profileService.SetLoadedProfile(ProfileMapper.ToProfileData(response.Profile));
            _badgeService?.EnqueueAwardedBadges(response.AwardedBadgeIds);

            _activeLessonId = null;
            onSuccess?.Invoke(result);
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
        
        public IEnumerator RegisterIncompleteLesson(
            float elapsedSeconds,
            Action onSuccess,
            Action<string> onError)
        {
            if (!_activeLessonId.HasValue)
            {
                onSuccess?.Invoke();
                yield break;
            }

            LessonId lessonId = _activeLessonId.Value;

            var request = new CompleteLessonRequest
            {
                LessonId = lessonId.ToString(),
                Completed = false,
                CorrectAnswersCount = 0,
                QuestionCount = 1,
                AwardedExperience = 0,
                AwardedCoins = 0,
                ElapsedSeconds = Mathf.Max(0f, elapsedSeconds)
            };

            ProfileAwardResponse response = null;
            string error = null;

            yield return _lessonClient.CompleteLesson(
                request,
                r => response = r,
                e => error = e);

            if (!string.IsNullOrWhiteSpace(error) || response == null || response.Profile == null)
            {
                onError?.Invoke(error);
                yield break;
            }

            _profileService.SetLoadedProfile(ProfileMapper.ToProfileData(response.Profile));
            _badgeService?.EnqueueAwardedBadges(response.AwardedBadgeIds);

            _activeLessonId = null;
            onSuccess?.Invoke();
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
