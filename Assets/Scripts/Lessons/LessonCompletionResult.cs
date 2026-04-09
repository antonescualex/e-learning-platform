using Data.StaticData.Item;

namespace Lessons
{
    public sealed class LessonCompletionResult
    {
        public LessonId LessonId { get; }
        public int TotalQuestions { get; }
        public int CorrectAnswers { get; }
        public float ElapsedSeconds { get; }
        public int CoinsReward { get; }
        public int ExperienceReward { get; }
        public BoosterDefinition AwardedBooster { get; }
        public RewardDefinition AwardedReward { get; }
        public bool HasSpecialItems => AwardedBooster != null || AwardedReward != null;

        public LessonCompletionResult(
            LessonId lessonId,
            int totalQuestions,
            int correctAnswers,
            float elapsedSeconds,
            int coinsReward,
            int experienceReward,
            BoosterDefinition awardedBooster = null,
            RewardDefinition awardedReward = null)
        {
            LessonId = lessonId;
            TotalQuestions = totalQuestions;
            CorrectAnswers = correctAnswers;
            ElapsedSeconds = elapsedSeconds;
            CoinsReward = coinsReward;
            ExperienceReward = experienceReward;
            AwardedBooster = awardedBooster;
            AwardedReward = awardedReward;
        }
    }
}
