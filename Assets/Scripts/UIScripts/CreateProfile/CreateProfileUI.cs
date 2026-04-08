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

        private void Start()
        {
            ServiceContainer.TryResolve<IProfileService>(out _profileService);
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
            SceneManager.LoadScene("MainMenu");
        }
    }
}
