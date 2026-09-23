using System;
using System.Collections.Generic;
using Data.StaticData.Lesson;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIScripts.Lessons.Views
{
    public class TextQuestionView : AbstractMultipleChoiceQuestionView
    {
        [Header("Header")]
        [SerializeField] private TMP_Text lessonTypeHeadlineText;

        [Header("Question")]
        [SerializeField] private TMP_Text questionText;
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

        private readonly List<TextQuestionDefinition> _sessionQuestions = new List<TextQuestionDefinition>();

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
            IReadOnlyList<TextQuestionDefinition> questions,
            int questionsPerSession,
            Action<int, int> onCompleted,
            Action onBackRequested)
        {
            InitializeSession(lessonHeadline, onCompleted, onBackRequested);
            _sessionQuestions.Clear();

            if (questions == null || questions.Count == 0)
            {
                Debug.LogWarning("TextQuestionView: runtime questions are empty.");
                FinishSession();
                return;
            }

            _sessionQuestions.AddRange(BuildSessionQuestions(questions, questionsPerSession, true));
            StartSession();
        }

        protected override void RenderQuestion(int questionIndex)
        {
            TextQuestionDefinition current = _sessionQuestions[questionIndex];
            if (questionText != null)
            {
                questionText.text = current.QuestionText;
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
    }
}
