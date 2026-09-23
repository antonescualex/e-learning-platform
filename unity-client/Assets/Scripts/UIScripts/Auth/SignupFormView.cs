using System.Collections;
using App;
using Auth;
using Clients.Interfaces;
using Dto.Auth;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIScripts.Auth
{
    public class SignupFormView : MonoBehaviour
    {
        [SerializeField] private AuthSceneController authSceneController;
        [SerializeField] private TMP_InputField usernameInputField;
        [SerializeField] private TMP_InputField passwordInputField;
        [SerializeField] private TMP_InputField playerNameInputField;
        [SerializeField] private Button signupButton;
        [SerializeField] private Button showLoginButton;

        private IAuthClient _authClient;
        private bool _isSubmitting;

        private void Awake()
        {
            if (signupButton != null)
            {
                signupButton.onClick.AddListener(OnSignupClicked);
                signupButton.interactable = false;
            }

            if (showLoginButton != null)
            {
                showLoginButton.onClick.AddListener(OnShowLoginClicked);
            }

            if (usernameInputField != null)
            {
                usernameInputField.onValueChanged.AddListener(_ => RefreshButtonState());
            }

            if (passwordInputField != null)
            {
                passwordInputField.onValueChanged.AddListener(_ => RefreshButtonState());
            }

            if (playerNameInputField != null)
            {
                playerNameInputField.onValueChanged.AddListener(_ => RefreshButtonState());
            }
        }

        private void Start()
        {
            _authClient = ServiceContainer.Resolve<IAuthClient>();
            RefreshButtonState();
        }

        private void OnShowLoginClicked()
        {
            if (_isSubmitting) return;
            authSceneController.ShowLogin();
        }

        private void OnSignupClicked()
        {
            if (_isSubmitting) return;
            StartCoroutine(SignupRoutine());
        }

        private IEnumerator SignupRoutine()
        {
            _isSubmitting = true;
            RefreshButtonState();
            authSceneController.SetStatus(string.Empty);

            string username = usernameInputField.text.Trim();
            string password = passwordInputField.text;
            string playerName = playerNameInputField.text.Trim();

            AuthTokensResponse tokens = null;
            string errorMessage = null;

            yield return _authClient.Register(
                username,
                password,
                playerName,
                response => tokens = response,
                error => errorMessage = error);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                authSceneController.SetStatus(errorMessage);
                _isSubmitting = false;
                RefreshButtonState();
                yield break;
            }

            yield return authSceneController.CompleteAuthentication(tokens);

            _isSubmitting = false;
            RefreshButtonState();
            
            authSceneController.ShowLogin();
            authSceneController.SetStatus("Account created. Please log in.");
        }

        private void RefreshButtonState()
        {
            if (signupButton == null) return;

            bool hasUsername = usernameInputField != null && !string.IsNullOrWhiteSpace(usernameInputField.text);
            bool hasPassword = passwordInputField != null && !string.IsNullOrWhiteSpace(passwordInputField.text);
            bool hasPlayerName = playerNameInputField != null && !string.IsNullOrWhiteSpace(playerNameInputField.text);

            signupButton.interactable = !_isSubmitting && hasUsername && hasPassword && hasPlayerName;
        }
    }
}