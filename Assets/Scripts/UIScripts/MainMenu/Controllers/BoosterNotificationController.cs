using System;
using App;
using Enums;
using Services.Interfaces;
using TMPro;
using UnityEngine;

namespace UIScripts.MainMenu.Controllers
{
    public class BoosterNotificationController : MonoBehaviour
    {
        [SerializeField] private GameObject boosterNotification;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text boosterTypeText;
        
        private IBoosterService _boosterService;

        private void Start()
        {
            ServiceContainer.TryResolve<IBoosterService>(out _boosterService);
            RefreshNotification();
        }
        
        private void Update()
        {
            RefreshNotification();
        }

        private void RefreshNotification()
        {
            if (_boosterService == null)
            {
                SetNotificationVisible(false);
                return;
            }

            if (_boosterService.IsBoosterActive(BoosterType.DoubleCoins))
            {
                ShowBooster(BoosterType.DoubleCoins);
                return;
            }

            if (_boosterService.IsBoosterActive(BoosterType.DoubleXP))
            {
                ShowBooster(BoosterType.DoubleXP);
                return;
            }

            SetNotificationVisible(false);
        }

        private void ShowBooster(BoosterType boosterType)
        {
            SetNotificationVisible(true);

            if (boosterTypeText != null)
            {
                boosterTypeText.text = boosterType == BoosterType.DoubleCoins
                    ? "Double Coins"
                    : "Double XP";
            }

            if (timerText != null)
            {
                timerText.text = FormatTime(_boosterService.GetRemainingTime(boosterType));
            }
        }

        private void SetNotificationVisible(bool visible)
        {
            if (boosterNotification != null)
            {
                boosterNotification.SetActive(visible);
            }
        }

        private string FormatTime(TimeSpan remainingTime)
        {
            if (remainingTime <= TimeSpan.Zero)
            {
                return "00:00";
            }

            if (remainingTime.TotalHours >= 1)
            {
                return $"{(int)remainingTime.TotalHours:00}:{remainingTime.Minutes:00}:{remainingTime.Seconds:00}";
            }

            return $"{remainingTime.Minutes:00}:{remainingTime.Seconds:00}";
        }
    }
}