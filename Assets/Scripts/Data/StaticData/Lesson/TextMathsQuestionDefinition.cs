using System;
using UnityEngine;

namespace Data.StaticData.Lesson
{
    [Serializable]
    public class TextMathsQuestionDefinition
    {
        [TextArea] public string QuestionText;
        public string[] Answers = new string[4];
        [Range(0, 3)] public int CorrectAnswerIndex;
    }
}