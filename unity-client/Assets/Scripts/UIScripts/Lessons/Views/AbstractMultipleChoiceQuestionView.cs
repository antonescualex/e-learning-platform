using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UIScripts.Bootstrap;
using UnityEngine;
using UnityEngine.UI;

namespace UIScripts.Lessons.Views
{
    public abstract class AbstractMultipleChoiceQuestionView : MonoBehaviour
    {
        private Action _onBackRequested;
        private Action<int, int> _onCompleted;
        private Color[] _neutralAnswerColors = Array.Empty<Color>();
        private Coroutine _answerFeedbackCoroutine;

        protected int CurrentQuestionIndex { get; private set; }
        protected int CorrectAnswers { get; private set; }
        protected bool IsWaitingForNextQuestion { get; private set; }

        protected abstract TMP_Text LessonTypeHeadlineText { get; }
        protected abstract TMP_Text ProgressText { get; }
        protected abstract Button[] AnswerButtons { get; }
        protected abstract TMP_Text[] AnswerTexts { get; }
        protected abstract Image[] AnswerButtonImages { get; }
        protected abstract float AnswerFeedbackDelay { get; }
        protected abstract Color CorrectAnswerColor { get; }
        protected abstract Color IncorrectAnswerColor { get; }
        protected abstract Button BackButton { get; }
        protected abstract int QuestionCount { get; }

        protected void InitializeSession(string lessonHeadline, Action<int, int> onCompleted, Action onBackRequested)
        {
            _onBackRequested = onBackRequested;
            _onCompleted = onCompleted;
            CurrentQuestionIndex = 0;
            CorrectAnswers = 0;
            IsWaitingForNextQuestion = false;
            StopAnswerFeedback();

            Button backButton = BackButton;
            if (backButton != null)
            {
                backButton.onClick.RemoveListener(OnBackClicked);
                backButton.onClick.AddListener(OnBackClicked);
            }

            TMP_Text headlineText = LessonTypeHeadlineText;
            if (headlineText != null)
            {
                headlineText.text = lessonHeadline;
            }

            SaveAnswerImageColors();
            WireAnswerButtons();
        }

        protected void StartSession()
        {
            RenderCurrentQuestion();
        }

        protected void FinishSession()
        {
            _onCompleted?.Invoke(QuestionCount, CorrectAnswers);
        }

        protected static List<TQuestion> BuildSessionQuestions<TQuestion>(
            IReadOnlyList<TQuestion> source,
            int targetCount,
            bool fillWithRepeats)
        {
            List<TQuestion> result = new List<TQuestion>();
            if (source == null || source.Count == 0)
            {
                return result;
            }

            int normalizedCount = Mathf.Max(1, targetCount);
            List<int> indexes = CreateShuffledIndexes(source.Count);

            int count = Mathf.Min(normalizedCount, source.Count);
            for (int i = 0; i < count; i++)
            {
                result.Add(source[indexes[i]]);
            }

            if (!fillWithRepeats)
            {
                return result;
            }

            while (result.Count < normalizedCount)
            {
                int randomIndex = UnityEngine.Random.Range(0, source.Count);
                result.Add(source[randomIndex]);
            }

            return result;
        }

        protected abstract void RenderQuestion(int questionIndex);
        protected abstract string[] GetAnswers(int questionIndex);
        protected abstract int GetCorrectAnswerIndex(int questionIndex);

        private void WireAnswerButtons()
        {
            Button[] answerButtons = AnswerButtons;
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

        private void SaveAnswerImageColors()
        {
            Image[] answerButtonImages = AnswerButtonImages;
            if (answerButtonImages == null)
            {
                _neutralAnswerColors = Array.Empty<Color>();
                return;
            }

            _neutralAnswerColors = new Color[answerButtonImages.Length];

            for (int i = 0; i < answerButtonImages.Length; i++)
            {
                _neutralAnswerColors[i] = answerButtonImages[i] != null
                    ? answerButtonImages[i].color
                    : Color.white;
            }
        }

        private void RenderCurrentQuestion()
        {
            if (CurrentQuestionIndex < 0 || CurrentQuestionIndex >= QuestionCount)
            {
                FinishSession();
                return;
            }

            UpdateProgressText();
            ResetAnswerButtonVisuals();
            RenderQuestion(CurrentQuestionIndex);
            RenderAnswerTexts(GetAnswers(CurrentQuestionIndex));
        }

        private void UpdateProgressText()
        {
            TMP_Text progressText = ProgressText;
            if (progressText == null) return;

            progressText.text = (CurrentQuestionIndex + 1) + " / " + QuestionCount;
        }

        private void RenderAnswerTexts(string[] answers)
        {
            TMP_Text[] answerTexts = AnswerTexts;
            if (answerTexts == null) return;

            for (int i = 0; i < answerTexts.Length; i++)
            {
                if (answerTexts[i] == null) continue;
                answerTexts[i].text = answers != null && i < answers.Length ? answers[i] : string.Empty;
            }
        }

        private void OnAnswerClicked(int selectedAnswerIndex)
        {
            if (IsWaitingForNextQuestion) return;
            if (CurrentQuestionIndex < 0 || CurrentQuestionIndex >= QuestionCount) return;

            bool isCorrectAnswer = selectedAnswerIndex == GetCorrectAnswerIndex(CurrentQuestionIndex);
            if (isCorrectAnswer)
            {
                CorrectAnswers++;
            }

            AudioManager.Instance?.PlayAnswerFeedback(isCorrectAnswer);
            IsWaitingForNextQuestion = true;
            ApplyAnswerFeedback(selectedAnswerIndex, isCorrectAnswer);

            StopAnswerFeedback();
            _answerFeedbackCoroutine = StartCoroutine(AdvanceAfterAnswerFeedback());
        }

        private void ApplyAnswerFeedback(int selectedAnswerIndex, bool isCorrectAnswer)
        {
            Image[] answerButtonImages = AnswerButtonImages;
            if (answerButtonImages == null || selectedAnswerIndex < 0 || selectedAnswerIndex >= answerButtonImages.Length) return;
            if (answerButtonImages[selectedAnswerIndex] == null) return;

            answerButtonImages[selectedAnswerIndex].color = isCorrectAnswer
                ? CorrectAnswerColor
                : IncorrectAnswerColor;
        }

        private IEnumerator AdvanceAfterAnswerFeedback()
        {
            yield return new WaitForSeconds(AnswerFeedbackDelay);

            CurrentQuestionIndex++;
            _answerFeedbackCoroutine = null;
            IsWaitingForNextQuestion = false;

            RenderCurrentQuestion();
        }

        private void ResetAnswerButtonVisuals()
        {
            Image[] answerButtonImages = AnswerButtonImages;
            if (answerButtonImages == null || _neutralAnswerColors == null) return;

            int count = Mathf.Min(answerButtonImages.Length, _neutralAnswerColors.Length);
            for (int i = 0; i < count; i++)
            {
                if (answerButtonImages[i] != null)
                {
                    answerButtonImages[i].color = _neutralAnswerColors[i];
                }
            }
        }

        private void StopAnswerFeedback()
        {
            if (_answerFeedbackCoroutine == null) return;

            StopCoroutine(_answerFeedbackCoroutine);
            _answerFeedbackCoroutine = null;
        }

        private void OnDisable()
        {
            StopAnswerFeedback();
            IsWaitingForNextQuestion = false;
        }

        private void OnBackClicked()
        {
            if (IsWaitingForNextQuestion) return;
            _onBackRequested?.Invoke();
        }

        private static List<int> CreateShuffledIndexes(int count)
        {
            List<int> indexes = new List<int>(count);
            for (int i = 0; i < count; i++)
            {
                indexes.Add(i);
            }

            for (int i = indexes.Count - 1; i > 0; i--)
            {
                int randomIndex = UnityEngine.Random.Range(0, i + 1);
                int temp = indexes[i];
                indexes[i] = indexes[randomIndex];
                indexes[randomIndex] = temp;
            }

            return indexes;
        }
    }
}
