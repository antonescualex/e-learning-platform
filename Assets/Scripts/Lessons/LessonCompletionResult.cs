using Data.StaticData.Item;

namespace Lessons
{
    public sealed class LessonCompletionResult
    {
        public LessonId LessonId { get; }
        public int TotalQuestions { get; }
        public int CorrectAnswers { get; }
        public int CoinsReward { get; }
        public int ExperienceReward { get; }
        public ItemDefinition AwardedBooster { get; }
        public ItemDefinition AwardedReward { get; }
        public bool HasSpecialItems => AwardedBooster != null || AwardedReward != null;

        public LessonCompletionResult(
            LessonId lessonId,
            int totalQuestions,
            int correctAnswers,
            int coinsReward,
            int experienceReward,
            ItemDefinition awardedBooster = null,
            ItemDefinition awardedReward = null)
        {
            LessonId = lessonId;
            TotalQuestions = totalQuestions;
            CorrectAnswers = correctAnswers;
            CoinsReward = coinsReward;
            ExperienceReward = experienceReward;
            AwardedBooster = awardedBooster;
            AwardedReward = awardedReward;
        }
    }
}
