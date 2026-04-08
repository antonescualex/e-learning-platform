using System;
using System.Collections.Generic;
using Data.StaticData.Lesson;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIScripts.Lessons.Views
{
    public class SyllableDivisionQuestionView : MonoBehaviour
    {
        [SerializeField] private TMP_Text lessonTypeHeadlineText;
        [SerializeField] private TMP_Text promptText;
        [SerializeField] private TMP_Text progressText;
        [SerializeField] private TMP_InputField answerInput;
        [SerializeField] private Button submitButton;
        [SerializeField] private Button backButton;

        private readonly List<SyllableDivisionQuestionDefinition> _questions = new List<SyllableDivisionQuestionDefinition>();

        private Action _onBackRequested;
        private Action<int, int> _onCompleted;
        private int _currentQuestionIndex;
        private int _correctAnswers;

        public void InitRuntime(
            string lessonHeadline,
            IReadOnlyList<SyllableDivisionQuestionDefinition> questions,
            Action<int, int> onCompleted,
            Action onBackRequested)
        {
            _onBackRequested = onBackRequested;
            _onCompleted = onCompleted;
            _currentQuestionIndex = 0;
            _correctAnswers = 0;
            _questions.Clear();

            if (questions != null) _questions.AddRange(questions);

            if (lessonTypeHeadlineText != null) lessonTypeHeadlineText.text = lessonHeadline;

            if (submitButton != null)
            {
                submitButton.onClick.RemoveAllListeners();
                submitButton.onClick.AddListener(OnSubmitClicked);
            }

            if (backButton != null)
            {
                backButton.onClick.RemoveAllListeners();
                backButton.onClick.AddListener(OnBackClicked);
            }

            RenderCurrentQuestion();
        }

        private void RenderCurrentQuestion()
        {
            if (_currentQuestionIndex >= _questions.Count)
            {
                _onCompleted?.Invoke(_questions.Count, _correctAnswers);
                return;
            }

            SyllableDivisionQuestionDefinition current = _questions[_currentQuestionIndex];

            if (promptText != null) promptText.text = current.PromptText;
            if (progressText != null) progressText.text = (_currentQuestionIndex + 1) + " / " + _questions.Count;
            if (answerInput != null) answerInput.text = string.Empty;
        }

        private void OnSubmitClicked()
        {
            if (_currentQuestionIndex >= _questions.Count) return;

            SyllableDivisionQuestionDefinition current = _questions[_currentQuestionIndex];
            string userAnswer = Normalize(answerInput != null ? answerInput.text : string.Empty);
            string expectedAnswer = Normalize(current.ExpectedAnswer);

            if (userAnswer == expectedAnswer)
            {
                _correctAnswers++;
            }

            _currentQuestionIndex++;
            RenderCurrentQuestion();
        }

        private string Normalize(string value)
        {
            return (value ?? string.Empty)
                .Trim()
                .Replace(" ", string.Empty)
                .Replace("_", "-")
                .ToLowerInvariant();
        }

        private void OnBackClicked()
        {
            _onBackRequested?.Invoke();
        }
    }
}