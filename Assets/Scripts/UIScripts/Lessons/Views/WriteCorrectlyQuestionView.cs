using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using App;
using Data.StaticData.Lesson;
using Services.Interfaces;
using SimpleOfflineTTS;
using TMPro;
using UIScripts.Bootstrap;
using UnityEngine;
using UnityEngine.UI;

namespace UIScripts.Lessons.Views
{
    public class WriteCorrectlyQuestionView : MonoBehaviour
    {
        [SerializeField] private TMP_Text lessonTypeHeadlineText;
        [SerializeField] private TMP_Text progressText;
        [SerializeField] private TMP_InputField answerInput;
        [SerializeField] private Button submitButton;
        [SerializeField] private Button repeatAudioButton;
        [SerializeField] private Button backButton;
        [SerializeField] private TTSVoice speechSystem;
        [SerializeField] private float playbackEndBuffer = 0.1f;

        private readonly List<WriteCorrectlyQuestionDefinition> _questions = new List<WriteCorrectlyQuestionDefinition>();

        private ISettingsService _settingsService;
        private bool _musicMuted;
        private Action _onBackRequested;
        private Action<int, int> _onCompleted;
        private int _currentQuestionIndex;
        private int _correctAnswers;
        private Coroutine _speechRoutine;

        public void InitRuntime(
            string lessonHeadline,
            IReadOnlyList<WriteCorrectlyQuestionDefinition> questions,
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

            if (repeatAudioButton != null)
            {
                repeatAudioButton.onClick.RemoveAllListeners();
                repeatAudioButton.onClick.AddListener(OnRepeatAudioClicked);
            }

            if (backButton != null)
            {
                backButton.onClick.RemoveAllListeners();
                backButton.onClick.AddListener(OnBackClicked);
            }

            MuteBackgroundMusic();
            RenderCurrentQuestion();
        }

        private void MuteBackgroundMusic()
        {
            if (AudioManager.Instance == null) return;
            if (!ServiceContainer.TryResolve<ISettingsService>(out _settingsService)) return;
            if (_settingsService.CurrentSettings == null) return;
            
            var mutedSettings = _settingsService.CurrentSettings.Copy();
            mutedSettings.MusicEnabled = false;
            
            AudioManager.Instance.ApplySettings(mutedSettings);
            _musicMuted = true;
        }

        private void RestoreAudioSettings()
        {
            if(AudioManager.Instance == null) return;
            
            if (!_musicMuted) return;
            _musicMuted = false;
            
            if (_settingsService == null) 
            {
                ServiceContainer.TryResolve<ISettingsService>(out _settingsService);
            }

            if (_settingsService?.CurrentSettings != null)
            {
                AudioManager.Instance.ApplySettings(_settingsService.CurrentSettings);
            }
        }
        
        private void RenderCurrentQuestion()
        {
            if (_currentQuestionIndex >= _questions.Count)
            {
                RestoreAudioSettings();
                _onCompleted?.Invoke(_questions.Count, _correctAnswers);
                return;
            }

            if (progressText != null) progressText.text = (_currentQuestionIndex + 1) + " / " + _questions.Count;

            if (answerInput != null)
            {
                answerInput.text = string.Empty;
                answerInput.Select();
                answerInput.ActivateInputField();
            }

            PlayCurrentSentence();
        }

        private void PlayCurrentSentence()
        {
            if (speechSystem == null) return;
            if (_currentQuestionIndex < 0 || _currentQuestionIndex >= _questions.Count) return;

            if (_speechRoutine != null)
            {
                StopCoroutine(_speechRoutine);
            }

            string sentence = _questions[_currentQuestionIndex].SentenceText;
            _speechRoutine = StartCoroutine(PlaySentenceAndLockUi(sentence));
        }

        private IEnumerator PlaySentenceAndLockUi(string sentence)
        {
            SetActionButtonsInteractable(false);

            yield return null;

            Task speakTask = speechSystem.Speak(sentence);

            while (!speakTask.IsCompleted)
            {
                yield return null;
            }

            if (speakTask.IsFaulted)
            {
                Debug.LogException(speakTask.Exception);
                SetActionButtonsInteractable(true);
                yield break;
            }

            while (speechSystem.IsSpeaking())
            {
                yield return null;
            }

            yield return new WaitForSeconds(playbackEndBuffer);

            SetActionButtonsInteractable(true);
            _speechRoutine = null;
        }

        private void SetActionButtonsInteractable(bool value)
        {
            if (submitButton != null) submitButton.interactable = value;
            if (repeatAudioButton != null) repeatAudioButton.interactable = value;
        }

        private void OnRepeatAudioClicked()
        {
            PlayCurrentSentence();
        }

        private void OnSubmitClicked()
        {
            if (_currentQuestionIndex >= _questions.Count) return;

            WriteCorrectlyQuestionDefinition current = _questions[_currentQuestionIndex];
            string userAnswer = Normalize(answerInput != null ? answerInput.text : string.Empty);
            string expectedAnswer = Normalize(current.ExpectedAnswer);
            bool isCorrect = userAnswer == expectedAnswer;

            if (isCorrect)
            {
                _correctAnswers++;
            }

            AudioManager.Instance?.PlayAnswerFeedback(isCorrect);
            _currentQuestionIndex++;
            RenderCurrentQuestion();
        }

        private static string Normalize(string value)
        {
            string normalized = (value ?? string.Empty)
                .Trim()
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Replace("\t", " ");

            normalized = Regex.Replace(normalized, @"\s+", " ");
            normalized = normalized.ToLowerInvariant();
            normalized = Regex.Replace(normalized, @"[.,!?;:'""]", string.Empty);
            return normalized.Trim();
        }

        private void OnBackClicked()
        {
            _onBackRequested?.Invoke();
        }

        private void OnDestroy()
        {
            RestoreAudioSettings();
        }
    }
}
