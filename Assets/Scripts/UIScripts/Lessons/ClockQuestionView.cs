using System;
using System.Collections;
using System.Collections.Generic;
using Data.StaticData.Lesson;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIScripts.Lessons
{
    public class ClockQuestionView : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] private TMP_Text lessonTypeHeadlineText;
        [SerializeField] private TMP_Text progressText;

        [Header("Clock")]
        [SerializeField] private RectTransform hourHandTransform;
        [SerializeField] private RectTransform minuteHandTransform;
        [SerializeField] private float hourHandZeroRotationOffset = 0f;
        [SerializeField] private float minuteHandZeroRotationOffset = 0f;

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

        private Action _onBackRequested;
        private Action<int, int> _onCompleted;
        private Color[] _neutralAnswerColors;
        private Coroutine _answerFeedbackCoroutine;
        private int _currentQuestionIndex;
        private int _correctAnswers;
        private bool _isWaitingForNextQuestion;

        public void InitRuntime(
            string lessonHeadline,
            IReadOnlyList<ClockQuestionDefinition> questions,
            int questionsPerSession,
            Action<int, int> onCompleted,
            Action onBackRequested)
        {
            _onBackRequested = onBackRequested;
            _onCompleted = onCompleted;
            _currentQuestionIndex = 0;
            _correctAnswers = 0;
            _isWaitingForNextQuestion = false;
            _sessionQuestions.Clear();
            StopAnswerFeedback();

            if (backButton != null)
            {
                backButton.onClick.RemoveListener(OnBackClicked);
                backButton.onClick.AddListener(OnBackClicked);
            }

            if (lessonTypeHeadlineText != null) lessonTypeHeadlineText.text = lessonHeadline;

            if (questions == null || questions.Count == 0)
            {
                _onCompleted?.Invoke(0, 0);
                return;
            }

            BuildSessionQuestions(questions, Mathf.Max(1, questionsPerSession));
            SaveAnswerImageColors();
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

        private void SaveAnswerImageColors()
        {
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

        private void BuildSessionQuestions(IReadOnlyList<ClockQuestionDefinition> source, int targetCount)
        {
            List<int> indexes = new List<int>(source.Count);
            for (int i = 0; i < source.Count; i++) indexes.Add(i);

            for (int i = indexes.Count - 1; i > 0; i--)
            {
                int randomIndex = UnityEngine.Random.Range(0, i + 1);
                int temp = indexes[i];
                indexes[i] = indexes[randomIndex];
                indexes[randomIndex] = temp;
            }

            int count = Mathf.Min(targetCount, source.Count);
            for (int i = 0; i < count; i++) _sessionQuestions.Add(source[indexes[i]]);
        }

        private void RenderCurrentQuestion()
        {
            if (_currentQuestionIndex < 0 || _currentQuestionIndex >= _sessionQuestions.Count)
            {
                _onCompleted?.Invoke(_sessionQuestions.Count, _correctAnswers);
                return;
            }

            ClockQuestionDefinition current = _sessionQuestions[_currentQuestionIndex];

            ApplyClock(current.Hour, current.Minute);

            if (progressText != null)
            {
                progressText.text = (_currentQuestionIndex + 1) + " / " + _sessionQuestions.Count;
            }

            ResetAnswerButtonVisuals();

            for (int i = 0; i < answerTexts.Length; i++)
            {
                if (answerTexts[i] == null) continue;
                answerTexts[i].text = (current.Answers != null && i < current.Answers.Length) ? current.Answers[i] : string.Empty;
            }
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

        private void OnAnswerClicked(int selectedAnswerIndex)
        {
            if (_isWaitingForNextQuestion) return;
            if (_currentQuestionIndex < 0 || _currentQuestionIndex >= _sessionQuestions.Count) return;

            ClockQuestionDefinition current = _sessionQuestions[_currentQuestionIndex];
            bool isCorrectAnswer = selectedAnswerIndex == current.CorrectAnswerIndex;

            if (isCorrectAnswer) _correctAnswers++;

            _isWaitingForNextQuestion = true;
            ApplyAnswerFeedback(selectedAnswerIndex, isCorrectAnswer);

            StopAnswerFeedback();
            _answerFeedbackCoroutine = StartCoroutine(AdvanceAfterAnswerFeedback());
        }

        private void ApplyAnswerFeedback(int selectedAnswerIndex, bool isCorrectAnswer)
        {
            if (answerButtonImages == null || selectedAnswerIndex < 0 || selectedAnswerIndex >= answerButtonImages.Length) return;
            if (answerButtonImages[selectedAnswerIndex] == null) return;

            answerButtonImages[selectedAnswerIndex].color = isCorrectAnswer
                ? correctAnswerColor
                : incorrectAnswerColor;
        }

        private IEnumerator AdvanceAfterAnswerFeedback()
        {
            yield return new WaitForSeconds(answerFeedbackDelay);

            _currentQuestionIndex++;
            _answerFeedbackCoroutine = null;

            if (_currentQuestionIndex >= _sessionQuestions.Count)
            {
                _onCompleted?.Invoke(_sessionQuestions.Count, _correctAnswers);
                yield break;
            }

            _isWaitingForNextQuestion = false;
            RenderCurrentQuestion();
        }

        private void ResetAnswerButtonVisuals()
        {
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
            _isWaitingForNextQuestion = false;
        }

        private void OnBackClicked()
        {
            if (_isWaitingForNextQuestion) return;
            _onBackRequested?.Invoke();
        }
    }
}
