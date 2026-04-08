using System;
using Data.StaticData.Item;
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
        [SerializeField] private Animator animator;
        [SerializeField] private GameObject imageWithShadow;
        [SerializeField] private Button clickButton;

        private Action _clickHandler;

        private void Awake()
        {
            if (clickButton != null)
            {
                clickButton.onClick.AddListener(OnClickPressed);
            }
        }

        private void OnDestroy()
        {
            if (clickButton != null)
            {
                clickButton.onClick.RemoveListener(OnClickPressed);
            }
        }

        public void Bind(ProfileItemDefinition itemDefinition)
        {
            if (itemDefinition == null)
            {
                Hide();
                return;
            }

            if (iconImage != null) iconImage.sprite = itemDefinition.Icon;
            if (shadowImage != null) shadowImage.sprite = itemDefinition.Icon;
            if (label != null) label.text = itemDefinition.DisplayName;
            if (spinner != null) spinner.gameObject.SetActive(false);
            gameObject.SetActive(true);
        }

        public void SetClickHandler(Action clickHandler)
        {
            _clickHandler = clickHandler;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void SetPopEnabled(bool enabled)
        {
            if (animator != null)
            {
                animator.SetBool("PlayPop", enabled);
            }
        }

        public void ShowWithoutPop()
        {
            gameObject.SetActive(true);
            if (imageWithShadow != null) imageWithShadow.transform.localScale = Vector3.one;

            if (animator != null)
            {
                animator.enabled = true;
                animator.SetBool("PlayPop", false);
                animator.Play("Idle", 0, 0f);
                animator.Update(0f);
            }
        }

        public void ShowWithPop()
        {
            gameObject.SetActive(true);

            if (animator != null)
            {
                animator.enabled = true;
                animator.SetBool("PlayPop", true);
                animator.Play("Pop", 0, 0f);
                animator.Update(0f);
            }
        }

        private void OnClickPressed()
        {
            _clickHandler?.Invoke();
        }
    }
}
