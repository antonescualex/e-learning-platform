using System;
using System.Collections.Generic;
using Data.StaticData.Lesson;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIScripts.Lessons
{
    public class TextMathsQuestionView : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] private TMP_Text lessonTypeHeadlineText;

        [Header("Question")]
        [SerializeField] private TMP_Text questionText;
        [SerializeField] private TMP_Text progressText;

        [Header("Answers")]
        [SerializeField] private Button[] answerButtons;
        [SerializeField] private TMP_Text[] answerTexts;

        [Header("Back Button")]
        [SerializeField] private Button backButton;

        private readonly List<TextMathsQuestionDefinition> _sessionQuestions = new List<TextMathsQuestionDefinition>();

        private Action _onBackRequested;
        private Action<int, int> _onCompleted;
        private int _currentQuestionIndex;
        private int _correctAnswers;

        public void Init(string lessonHeadline, TextMathsQuestionSet questionSet, int questionsPerSession, Action<int, int> onCompleted, Action onBackRequested)
        {
            _onBackRequested = onBackRequested;
            _onCompleted = onCompleted;
            _currentQuestionIndex = 0;
            _correctAnswers = 0;
            _sessionQuestions.Clear();

            if (backButton != null)
            {
                backButton.onClick.RemoveListener(OnBackClicked);
                backButton.onClick.AddListener(OnBackClicked);
            }

            if (lessonTypeHeadlineText != null) lessonTypeHeadlineText.text = lessonHeadline;

            if (questionSet == null || questionSet.Questions == null || questionSet.Questions.Count == 0)
            {
                Debug.LogWarning("TextMathsQuestionView: question set is empty.");
                _onCompleted?.Invoke(0, 0);
                return;
            }

            BuildSessionQuestions(questionSet.Questions, Mathf.Max(1, questionsPerSession));
            WireAnswerButtons();
            RenderCurrentQuestion();
        }

        private void WireAnswerButtons()
        {
            if (answerButtons == null) return;

            for (int i = 0; i < answerButtons.Length; i++)
            {
                int capturedIndex = i;
                Button button = answerButtons[i];
                if (button == null) continue;

                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => OnAnswerClicked(capturedIndex));
            }
        }

        private void BuildSessionQuestions(IReadOnlyList<TextMathsQuestionDefinition> source, int targetCount)
        {
            List<int> indexes = new List<int>(source.Count);
            for (int i = 0; i < source.Count; i++) indexes.Add(i);
            Shuffle(indexes);

            int count = Mathf.Min(targetCount, source.Count);
            for (int i = 0; i < count; i++) _sessionQuestions.Add(source[indexes[i]]);

            while (_sessionQuestions.Count < targetCount)
            {
                int randomIndex = UnityEngine.Random.Range(0, source.Count);
                _sessionQuestions.Add(source[randomIndex]);
            }
        }

        private void RenderCurrentQuestion()
        {
            if (_currentQuestionIndex < 0 || _currentQuestionIndex >= _sessionQuestions.Count)
            {
                FinishLesson();
                return;
            }

            TextMathsQuestionDefinition current = _sessionQuestions[_currentQuestionIndex];

            if (questionText != null) questionText.text = current.QuestionText;
            if (progressText != null) progressText.text = (_currentQuestionIndex + 1) + " / " + _sessionQuestions.Count;

            for (int i = 0; i < answerTexts.Length; i++)
            {
                if (answerTexts[i] == null) continue;
                answerTexts[i].text = (current.Answers != null && i < current.Answers.Length) ? current.Answers[i] : string.Empty;
            }
        }

        private void OnAnswerClicked(int selectedAnswerIndex)
        {
            if (_currentQuestionIndex < 0 || _currentQuestionIndex >= _sessionQuestions.Count) return;

            TextMathsQuestionDefinition current = _sessionQuestions[_currentQuestionIndex];
            if (selectedAnswerIndex == current.CorrectAnswerIndex) _correctAnswers++;

            _currentQuestionIndex++;

            if (_currentQuestionIndex >= _sessionQuestions.Count)
            {
                FinishLesson();
                return;
            }

            RenderCurrentQuestion();
        }

        private void FinishLesson()
        {
            _onCompleted?.Invoke(_sessionQuestions.Count, _correctAnswers);
        }

        private static void Shuffle(List<int> items)
        {
            for (int i = items.Count - 1; i > 0; i--)
            {
                int randomIndex = UnityEngine.Random.Range(0, i + 1);
                int temp = items[i];
                items[i] = items[randomIndex];
                items[randomIndex] = temp;
            }
        }

        private void OnBackClicked()
        {
            _onBackRequested?.Invoke();
        }

    }
}