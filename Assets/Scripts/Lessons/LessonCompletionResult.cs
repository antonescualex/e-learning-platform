using Enums;

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

        public LessonCompletionResult(
            LessonId lessonId,
            int totalQuestions,
            int correctAnswers,
            float elapsedSeconds,
            int coinsReward,
            int experienceReward)
        {
            LessonId = lessonId;
            TotalQuestions = totalQuestions;
            CorrectAnswers = correctAnswers;
            ElapsedSeconds = elapsedSeconds;
            CoinsReward = coinsReward;
            ExperienceReward = experienceReward;
        }
    }
}
