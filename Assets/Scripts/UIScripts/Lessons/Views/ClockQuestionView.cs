using System;
using System.Collections.Generic;
using Data.StaticData.Lesson;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIScripts.Lessons.Views
{
    public class ClockQuestionView : AbstractMultipleChoiceQuestionView
    {
        [Header("Header")]
        [SerializeField] private TMP_Text lessonTypeHeadlineText;
        [SerializeField] private TMP_Text progressText;

        [Header("Clock")]
        [SerializeField] private RectTransform hourHandTransform;
        [SerializeField] private RectTransform minuteHandTransform;
        [SerializeField] private float hourHandZeroRotationOffset;
        [SerializeField] private float minuteHandZeroRotationOffset;

        [Header("Answers")]
        [SerializeField] private Button[] answerButtons;
        [SerializeField] private TMP_Text[] answerTexts;
        [SerializeField] private Image[] answerButtonImages;
        [SerializeField] private float answerFeedbackDelay = 1f;
        [SerializeField] private Color correctAnswerColor;
        [SerializeField] private Color incorrectAnswerColor;

        [Header("Back Button")]
        [SerializeField] private Button backButton;

        private readonly List<ClockQuestionDefinition> _sessionQuestions = new List<ClockQuestionDefinition>();

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
            IReadOnlyList<ClockQuestionDefinition> questions,
            int questionsPerSession,
            Action<int, int> onCompleted,
            Action onBackRequested)
        {
            InitializeSession(lessonHeadline, onCompleted, onBackRequested);
            _sessionQuestions.Clear();

            if (questions == null || questions.Count == 0)
            {
                FinishSession();
                return;
            }

            _sessionQuestions.AddRange(BuildSessionQuestions(questions, questionsPerSession, false));
            StartSession();
        }

        protected override void RenderQuestion(int questionIndex)
        {
            ClockQuestionDefinition current = _sessionQuestions[questionIndex];
            ApplyClock(current.Hour, current.Minute);
        }

        protected override string[] GetAnswers(int questionIndex)
        {
            return _sessionQuestions[questionIndex].Answers;
        }

        protected override int GetCorrectAnswerIndex(int questionIndex)
        {
            return _sessionQuestions[questionIndex].CorrectAnswerIndex;
        }

        private void ApplyClock(int hour24, int minute)
        {
            minute = Mathf.Clamp(minute, 0, 59);
            hour24 = ((hour24 % 24) + 24) % 24;

            float minuteAngle = minuteHandZeroRotationOffset - minute * 6f;
            float hourAngle = hourHandZeroRotationOffset - ((hour24 % 12) * 30f + minute * 0.5f);

            if (minuteHandTransform != null)
            {
                minuteHandTransform.localEulerAngles = new Vector3(0f, 0f, minuteAngle);
            }

            if (hourHandTransform != null)
            {
                hourHandTransform.localEulerAngles = new Vector3(0f, 0f, hourAngle);
            }
        }
    }
}
