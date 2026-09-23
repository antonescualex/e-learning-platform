using System;
using UnityEngine;

namespace Data.StaticData.Lesson
{
    [Serializable]
    public class ClockQuestionDefinition
    {
        [Range(0, 23)] public int Hour;
        [Range(0, 59)] public int Minute;
        public string[] Answers = new string[4];
        [Range(0, 3)] public int CorrectAnswerIndex;
    }
}