using System;

namespace Dto.Lesson
{
    [Serializable]
    public sealed class CompleteLessonRequest
    {
        public string LessonId;
        public bool Completed;
        public int CorrectAnswersCount;
        public int QuestionCount;
        public int AwardedExperience;
        public int AwardedCoins;
        public float ElapsedSeconds;
    }
}