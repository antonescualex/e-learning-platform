using System;
using System.Collections.Generic;
using Data.StaticData.Lesson;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIScripts.Lessons.Views
{
    public class ShapesQuestionView : AbstractMultipleChoiceQuestionView
    {
        [Header("Header")]
        [SerializeField] private TMP_Text lessonTypeHeadlineText;

        [Header("Question")]
        [SerializeField] private Image questionImage;
        [SerializeField] private TMP_Text progressText;

        [Header("Answers")]
        [SerializeField] private Button[] answerButtons;
        [SerializeField] private TMP_Text[] answerTexts;
        [SerializeField] private Image[] answerButtonImages;
        [SerializeField] private float answerFeedbackDelay = 1f;
        [SerializeField] private Color correctAnswerColor;
        [SerializeField] private Color incorrectAnswerColor;

        [Header("Back Button")]
        [SerializeField] private Button backButton;

        private readonly List<SessionQuestion> _sessionQuestions = new List<SessionQuestion>();

        protected override TMP_Text LessonTypeHeadlineText => lessonTypeHeadlineText;
        protected override TMP_Text ProgressText => progressText;
        protected override Button[] AnswerButtons => answerButtons;
        protected override TMP_Text[] AnswerTexts => answerTexts;
        protected override Image[] AnswerButtonImages => answerButtonImages;
        protected override float AnswerFeedbackDelay => answerFeedbackDelay;
        protected override Color CorrectAnswerColor => correctAnswerColor;
        protected override Color IncorrectAnswerColor => incorrectAnswerColor;
        protected override Button BackButton => backButton;
        protected override int QuestionCount => _sessionQuestions.Count;

        public void InitRuntime(
            string lessonHeadline,
            IReadOnlyList<ShapeQuestionDefinition> questions,
            ShapeSpriteCatalog shapeSpriteCatalog,
            int questionsPerSession,
            Action<int, int> onCompleted,
            Action onBackRequested)
        {
            InitializeSession(lessonHeadline, onCompleted, onBackRequested);
            _sessionQuestions.Clear();

            if (questions == null || questions.Count == 0)
            {
                Debug.LogWarning("ShapesQuestionView: runtime questions are empty.");
                FinishSession();
                return;
            }

            List<SessionQuestion> resolvedQuestions = CreateRuntimeQuestions(questions, shapeSpriteCatalog);
            if (resolvedQuestions.Count == 0)
            {
                Debug.LogWarning("ShapesQuestionView: no runtime questions could be resolved.");
                FinishSession();
                return;
            }

            _sessionQuestions.AddRange(BuildSessionQuestions(resolvedQuestions, questionsPerSession, true));
            StartSession();
        }

        protected override void RenderQuestion(int questionIndex)
        {
            SessionQuestion current = _sessionQuestions[questionIndex];

            if (questionImage != null)
            {
                questionImage.sprite = current.QuestionSprite;
                questionImage.type = Image.Type.Simple;
                questionImage.preserveAspect = true;
                questionImage.enabled = current.QuestionSprite != null;
            }
        }

        protected override string[] GetAnswers(int questionIndex)
        {
            return _sessionQuestions[questionIndex].Answers;
        }

        protected override int GetCorrectAnswerIndex(int questionIndex)
        {
            return _sessionQuestions[questionIndex].CorrectAnswerIndex;
        }

        private List<SessionQuestion> CreateRuntimeQuestions(IReadOnlyList<ShapeQuestionDefinition> source, ShapeSpriteCatalog shapeSpriteCatalog)
        {
            List<SessionQuestion> result = new List<SessionQuestion>();
            if (shapeSpriteCatalog == null) return result;

            for (int i = 0; i < source.Count; i++)
            {
                ShapeQuestionDefinition question = source[i];
                if (question == null || string.IsNullOrWhiteSpace(question.ShapeId)) continue;

                Sprite sprite = shapeSpriteCatalog.GetSprite(question.ShapeId);
                if (sprite == null)
                {
                    Debug.LogWarning("ShapesQuestionView: missing sprite for shape id '" + question.ShapeId + "'.");
                    continue;
                }

                result.Add(new SessionQuestion
                {
                    QuestionSprite = sprite,
                    Answers = CopyAnswers(question.Answers),
                    CorrectAnswerIndex = Mathf.Clamp(question.CorrectAnswerIndex, 0, 3)
                });
            }

            return result;
        }

        private static string[] CopyAnswers(string[] source)
        {
            string[] result = new string[4];

            if (source == null) return result;

            int count = Mathf.Min(4, source.Length);
            for (int i = 0; i < count; i++)
            {
                result[i] = source[i];
            }

            return result;
        }

        private sealed class SessionQuestion
        {
            public Sprite QuestionSprite;
            public string[] Answers = new string[4];
            public int CorrectAnswerIndex;
        }
    }
}
