using Data.StaticData.Item;
using Enums;
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

        public void Bind(BoosterDefinition boosterDefinition)
        {
            if (boosterDefinition == null) return;

            CreateBoosterName(boosterDefinition);
            CreateBoosterDescription(boosterDefinition);
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