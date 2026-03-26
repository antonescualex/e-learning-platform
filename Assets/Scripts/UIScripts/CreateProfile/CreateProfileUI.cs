using System;
using Services;
using Services.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UIScripts.CreateProfile
{
    public class CreateProfileUI : MonoBehaviour
    {
        [SerializeField] private TMP_InputField playerNameInputField;
        [SerializeField] private Button signUpButton;

        private IProfileService _profileService;

        private void Start()
        {
            _profileService = App.Instance.ProfileService;
        }

        private void Awake()
        {
            signUpButton.onClick.AddListener(OnEnterClicked);
            signUpButton.interactable = false;
            playerNameInputField.onValueChanged.AddListener(OnInputChanged);
        }

        private void OnInputChanged(string newText)
        {
            signUpButton.interactable = !string.IsNullOrWhiteSpace(newText);
        }

        private void OnEnterClicked()
        {
            App.Instance.ProfileService.CreateNewProfile(playerNameInputField.text.Trim());
            SceneManager.LoadScene("MainMenu");
        }
    }
}
