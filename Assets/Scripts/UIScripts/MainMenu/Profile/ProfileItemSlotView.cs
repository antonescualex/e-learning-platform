using Data.StaticData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIScripts.MainMenu.Profile
{
    public class ProfileItemSlotView : MonoBehaviour
    {
        [SerializeField] private Image spinner;
        [SerializeField] private Image iconImage;
        [SerializeField] private Image shadowImage;
        [SerializeField] private TMP_Text label;

        public void Bind(ItemDefinition itemDefinition)
        {
            if (iconImage != null && shadowImage != null)
            {
                iconImage.sprite = itemDefinition.Icon;
                shadowImage.sprite = itemDefinition.Icon;
            }
            if (label != null) label.text = itemDefinition.DisplayName;
            spinner.gameObject.SetActive(false);
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}