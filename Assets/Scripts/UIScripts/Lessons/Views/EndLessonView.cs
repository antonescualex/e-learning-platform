using System;
using Data.StaticData.Item;
using Lessons;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIScripts.Lessons.Views
{
    public class EndLessonView : MonoBehaviour
    {
        [Header("Texts")]
        [SerializeField] private TMP_Text experienceAmountText;
        [SerializeField] private TMP_Text coinsText;

        [Header("Buttons")]
        [SerializeField] private Button restartButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private Button closeButton;

        [Header("Stars")]
        [SerializeField] private Image[] starImages;
        [SerializeField] private GameObject[] starShines;
        [SerializeField] private Color enabledStarColor = Color.white;
        [SerializeField] private Color disabledStarColor = new Color32(70, 70, 70, 255);

        [Header("Special Rewards")]
        [SerializeField] private GameObject boosterRewardObject;
        [SerializeField] private Image boosterRewardIcon;
        [SerializeField] private TMP_Text boosterRewardText;
        [SerializeField] private GameObject giftRewardObject;
        [SerializeField] private Image giftRewardIcon;
        [SerializeField] private TMP_Text giftRewardText;

        private Action _onRestart;
        private Action _onExit;

        public void Init(LessonCompletionResult result, Action onRestart, Action onExit)
        {
            _onRestart = onRestart;
            _onExit = onExit;

            if (experienceAmountText != null) experienceAmountText.text = result.ExperienceReward.ToString();
            if (coinsText != null) coinsText.text = result.CoinsReward.ToString();

            ApplySpecialRewardSlots(result);

            BindButton(restartButton, HandleRestartClicked);
            BindButton(exitButton, HandleExitClicked);
            BindButton(closeButton, HandleExitClicked);

            ApplyStars(result.CorrectAnswers, result.TotalQuestions);
            Debug.Log($"EndLessonView: correct={result.CorrectAnswers}, total={result.TotalQuestions}");
        }

        private void ApplySpecialRewardSlots(LessonCompletionResult result)
        {
            SetRewardSlot(boosterRewardObject, boosterRewardIcon, boosterRewardText, result?.AwardedBooster);
            SetRewardSlot(giftRewardObject, giftRewardIcon, giftRewardText, result?.AwardedReward);
        }

        private static void SetRewardSlot(GameObject slotObject, Image slotIcon, TMP_Text slotText, ProfileItemDefinition item)
        {
            if (slotObject == null) return;

            bool hasItem = item != null;

            if (slotIcon != null)
            {
                slotIcon.sprite = hasItem ? item.Icon : null;
                slotIcon.enabled = hasItem && item.Icon != null;
            }

            if (slotText != null)
            {
                slotText.text = hasItem ? item.DisplayName : string.Empty;
            }

            slotObject.SetActive(hasItem);
        }

        private void BindButton(Button button, Action action)
        {
            if (button == null) return;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => action?.Invoke());
        }

        private void ApplyStars(int correctAnswers, int totalQuestions)
        {
            int earnedStars = CalculateEarnedStars(correctAnswers);

            for (int i = 0; i < starImages.Length; i++)
            {
                bool earned = i < earnedStars;

                if (starImages[i] != null)
                    starImages[i].color = earned ? enabledStarColor : disabledStarColor;

                if (starShines != null && i < starShines.Length && starShines[i] != null)
                    starShines[i].SetActive(earned);
            }
        }

        private int CalculateEarnedStars(int correctAnswers)
        {
            correctAnswers = Mathf.Clamp(correctAnswers, 0, 5);

            if (correctAnswers <= 1) return 1;
            if (correctAnswers <= 3) return 2;
            return 3;
        }

        private void HandleRestartClicked() => _onRestart?.Invoke();
        private void HandleExitClicked() => _onExit?.Invoke();
    }
}
