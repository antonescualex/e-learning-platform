using System.Collections.Generic;
using UnityEngine;

namespace Data.StaticData.Lesson
{
    [CreateAssetMenu(menuName = "Lessons/Maths/Text Question Set")]
    public class TextMathsQuestionSet : ScriptableObject
    {
        [SerializeField] private List<TextMathsQuestionDefinition> questions = new List<TextMathsQuestionDefinition>();
        public IReadOnlyList<TextMathsQuestionDefinition> Questions => questions;

        private void OnValidate()
        {
            if (questions == null) return;

            foreach (TextMathsQuestionDefinition question in questions)
            {
                if (question == null) continue;

                if (question.Answers == null || question.Answers.Length != 4)
                {
                    string[] resized = new string[4];
                    if (question.Answers != null)
                    {
                        int count = Mathf.Min(4, question.Answers.Length);
                        for (int i = 0; i < count; i++) resized[i] = question.Answers[i];
                    }
                    question.Answers = resized;
                }

                question.CorrectAnswerIndex = Mathf.Clamp(question.CorrectAnswerIndex, 0, 3);
            }
        }
    }
}