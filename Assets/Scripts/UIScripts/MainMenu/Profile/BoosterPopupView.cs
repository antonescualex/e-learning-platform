using System.Collections;
using App;
using Data.StaticData.Item;
using Enums;
using Services.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIScripts.MainMenu.Profile
{
    public class BoosterPopupView : MonoBehaviour
    {
        [SerializeField] private TMP_Text boosterNameText;
        [SerializeField] private Image boosterImage;
        [SerializeField] private TMP_Text boosterDescriptionText;
        [SerializeField] private Button okButton;

        private BoosterDefinition _boosterDefinition;
        private bool _isActivating;
        
        public void Bind(BoosterDefinition boosterDefinition)
        {
            if (boosterDefinition == null) return;
            
            _boosterDefinition = boosterDefinition;

            CreateBoosterName(boosterDefinition);
            CreateBoosterDescription(boosterDefinition);
            
            if (okButton != null)
            {
                okButton.onClick.RemoveAllListeners();
                okButton.onClick.AddListener(OnOkPressed);
            }
        }

        private void OnOkPressed()
        {
            if (_boosterDefinition == null) return;
            if (_isActivating) return;

            if (!ServiceContainer.TryResolve<IBoosterService>(out IBoosterService boosterService))
                return;

            StartCoroutine(ActivateBooster(boosterService));
        }
        
        private IEnumerator ActivateBooster(IBoosterService boosterService)
        {
            _isActivating = true;

            if (okButton != null)
            {
                okButton.interactable = false;
            }

            bool activated = false;
            string error = null;

            yield return boosterService.ActivateBooster(
                _boosterDefinition.Id,
                () => activated = true,
                message => error = message);

            _isActivating = false;

            if (okButton != null)
            {
                okButton.interactable = true;
            }

            if (!activated)
            {
                Debug.LogWarning("Activate booster failed:\n" + error);
                yield break;
            }

            GetComponent<ProfileItemPopup>()?.Close();
        }

        public void CreateBoosterDescription(BoosterDefinition boosterDefinition)
        {
            if(boosterDefinition == null || boosterDescriptionText == null) return;
            
            string boosterTypeText = boosterDefinition.BoosterType == BoosterType.DoubleCoins ? "double coins" : "double xp";
            string durationText;
            if(boosterDefinition.DurationSeconds == 600)
                durationText = "10 minutes";
            else if(boosterDefinition.DurationSeconds == 1800)
                durationText = "30 minutes";
            else durationText = "1 hour";

            boosterDescriptionText.text = $"You are going to receive {boosterTypeText} for the next {durationText}.";
        }

        public void CreateBoosterName(BoosterDefinition boosterDefinition)
        {
            if(boosterDefinition == null || boosterNameText == null) return;
            
            string boosterTypeText = boosterDefinition.BoosterType == BoosterType.DoubleCoins ? "Double Coins" : "Double XP";
            string durationText;
            if(boosterDefinition.DurationSeconds == 600)
                durationText = "10m";
            else if(boosterDefinition.DurationSeconds == 1800)
                durationText = "30m";
            else durationText = "1h";
            
            boosterNameText.text = $"{boosterTypeText}({durationText})";
        }
    }
}