using System;
using System.Collections;
using System.Collections.Generic;
using Data.StaticData.Lesson;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIScripts.Lessons
{
    public class ShapesQuestionView : MonoBehaviour
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

        private Action _onBackRequested;
        private Action<int, int> _onCompleted;
        private Color[] _neutralAnswerColors;
        private Coroutine _answerFeedbackCoroutine;
        private int _currentQuestionIndex;
        private int _correctAnswers;
        private bool _isWaitingForNextQuestion;

        public void InitRuntime(
            string lessonHeadline,
            IReadOnlyList<ShapeQuestionDefinition> questions,
            ShapeSpriteCatalog shapeSpriteCatalog,
            int questionsPerSession,
            Action<int, int> onCompleted,
            Action onBackRequested)
        {
            PrepareSession(lessonHeadline, onCompleted, onBackRequested);

            if (questions == null || questions.Count == 0)
            {
                Debug.LogWarning("ShapesQuestionView: runtime questions are empty.");
                _onCompleted?.Invoke(0, 0);
                return;
            }

            List<SessionQuestion> resolvedQuestions = CreateRuntimeQuestions(questions, shapeSpriteCatalog);
            if (resolvedQuestions.Count == 0)
            {
                Debug.LogWarning("ShapesQuestionView: no runtime questions could be resolved.");
                _onCompleted?.Invoke(0, 0);
                return;
            }

            BuildSessionQuestions(resolvedQuestions, Mathf.Max(1, questionsPerSession));
            SaveAnswerImageColors();
            WireAnswerButtons();
            RenderCurrentQuestion();
        }

        private void PrepareSession(string lessonHeadline, Action<int, int> onCompleted, Action onBackRequested)
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

        private void BuildSessionQuestions(IReadOnlyList<SessionQuestion> source, int targetCount)
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

        private void RenderCurrentQuestion()
        {
            if (_currentQuestionIndex < 0 || _currentQuestionIndex >= _sessionQuestions.Count)
            {
                FinishLesson();
                return;
            }

            SessionQuestion current = _sessionQuestions[_currentQuestionIndex];

            if (questionImage != null)
            {
                questionImage.sprite = current.QuestionSprite;
                questionImage.type = Image.Type.Simple;
                questionImage.preserveAspect = true;
                questionImage.enabled = current.QuestionSprite != null;
            }

            if (progressText != null)
            {
                progressText.text = (_currentQuestionIndex + 1) + " / " + _sessionQuestions.Count;
            }

            ResetAnswerButtonVisuals();

            for (int i = 0; i < answerTexts.Length; i++)
            {
                if (answerTexts[i] == null) continue;
                answerTexts[i].text = i < current.Answers.Length ? current.Answers[i] : string.Empty;
            }
        }

        private void OnAnswerClicked(int selectedAnswerIndex)
        {
            if (_isWaitingForNextQuestion) return;
            if (_currentQuestionIndex < 0 || _currentQuestionIndex >= _sessionQuestions.Count) return;

            SessionQuestion current = _sessionQuestions[_currentQuestionIndex];
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
                FinishLesson();
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

        private void FinishLesson()
        {
            _onCompleted?.Invoke(_sessionQuestions.Count, _correctAnswers);
        }

        private void OnDisable()
        {
            StopAnswerFeedback();
            _isWaitingForNextQuestion = false;
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
            if (_isWaitingForNextQuestion) return;
            _onBackRequested?.Invoke();
        }

        private sealed class SessionQuestion
        {
            public Sprite QuestionSprite;
            public string[] Answers = new string[4];
            public int CorrectAnswerIndex;
        }
    }
}
