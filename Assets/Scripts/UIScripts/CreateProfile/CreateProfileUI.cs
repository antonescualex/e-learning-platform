using App;
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
        private IBadgeService _badgeService;

        private void Start()
        {
            ServiceContainer.TryResolve<IProfileService>(out _profileService);
            ServiceContainer.TryResolve<IBadgeService>(out _badgeService);
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
            if (_profileService == null) return;

            _profileService.CreateNewProfile(playerNameInputField.text.Trim());
            _badgeService?.HandleAppOpened();
            SceneManager.LoadScene("MainMenu");
        }
    }
}
