using Enums;
using Lessons;

namespace Services
{
    public class LessonService : ILessonService
    {
        private readonly IProfileService _profileService;

        private readonly int _baseCoinsReward = 10;
        private readonly int _baseExperienceReward = 20;
        private readonly int _coinsPerCorrectAnswer = 2;
        private readonly int _experiencePerCorrectAnswer = 4;

        private LessonId? _activeLessonId;

        public LessonService(IProfileService profileService)
        {
            _profileService = profileService;
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

        public void CompleteLesson(int totalQuestions, int correctAnswers)
        {
            if (!_activeLessonId.HasValue) return;

            if (totalQuestions < 0) totalQuestions = 0;
            if (correctAnswers < 0) correctAnswers = 0;
            if (correctAnswers > totalQuestions) correctAnswers = totalQuestions;

            int coinsReward = _baseCoinsReward + correctAnswers * _coinsPerCorrectAnswer;
            int experienceReward = _baseExperienceReward + correctAnswers * _experiencePerCorrectAnswer;

            _profileService?.AddCoins(coinsReward);
            _profileService?.AddExperience(experienceReward);

            _activeLessonId = null;
        }

        public void CancelLesson()
        {
            _activeLessonId = null;
        }
    }
}