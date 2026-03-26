using System;
using UnityEngine;

namespace Data.StaticData.Lesson
{
    [Serializable]
    public class WriteCorrectlyQuestionDefinition
    {
        [TextArea] public string SentenceText;
        public string ExpectedAnswer;
    }
}
