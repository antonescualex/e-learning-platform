using System.Collections.Generic;
using UnityEngine;

namespace Data.StaticData.Lesson
{
    [CreateAssetMenu(menuName = "Lessons/Maths/Image Question Set")]
    public class ImageMathsQuestionSet : ScriptableObject
    {
        [SerializeField] private List<ImageMathsQuestionDefinition> questions = new List<ImageMathsQuestionDefinition>();
        public IReadOnlyList<ImageMathsQuestionDefinition> Questions => questions;

        private void OnValidate()
        {
            if (questions == null) return;

            foreach (ImageMathsQuestionDefinition question in questions)
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