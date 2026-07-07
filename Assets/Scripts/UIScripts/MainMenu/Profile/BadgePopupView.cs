using Data.StaticData.Badge;
using Data.StaticData.Item;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIScripts.MainMenu.Profile
{
    public class BadgePopupView : MonoBehaviour
    {
        [SerializeField] private Image badgeSpriteImage;
        [SerializeField] private TMP_Text descriptionText;

        public void Bind(BadgeDefinition badgeDefinition)
        {
            if (badgeDefinition == null) return;

            if (badgeSpriteImage != null)
            {
                badgeSpriteImage.sprite = badgeDefinition.Icon;
                badgeSpriteImage.enabled = badgeDefinition.Icon != null;
            }

            if (descriptionText != null)
            {
                descriptionText.text = badgeDefinition.Description ?? string.Empty;
            }
        }
    }
}
