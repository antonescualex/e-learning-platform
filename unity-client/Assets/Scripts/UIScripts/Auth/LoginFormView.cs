using System.Collections;
using App;
using Clients.Interfaces;
using Dto.Auth;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UIScripts.Auth
{
    public class LoginFormView : MonoBehaviour
    {
        [SerializeField] private AuthSceneController authSceneController;
        [SerializeField] private TMP_InputField usernameInputField;
        [SerializeField] private TMP_InputField passwordInputField;
        [SerializeField] private Button loginButton;
        [SerializeField] private Button showSignupButton;
        [SerializeField] private GameObject successPopup;
        [SerializeField] private GameObject failPopup;
        [FormerlySerializedAs("successPopupDuration")] [SerializeField] private float popupDuration = 2f;

        private IAuthClient _authClient;
        private bool _isSubmitting;

        private void Awake()
        {
            if(successPopup != null) successPopup.SetActive(false);
            if(failPopup != null) failPopup.SetActive(false);
            
            if (loginButton != null)
            {
                loginButton.onClick.AddListener(OnLoginClicked);
                loginButton.interactable = false;
            }

            if (showSignupButton != null)
            {
                showSignupButton.onClick.AddListener(OnShowSignupClicked);
            }

            if (usernameInputField != null)
            {
                usernameInputField.onValueChanged.AddListener(_ => RefreshButtonState());
            }

            if (passwordInputField != null)
            {
                passwordInputField.onValueChanged.AddListener(_ => RefreshButtonState());
            }
        }

        private void Start()
        {
            _authClient = ServiceContainer.Resolve<IAuthClient>();
            RefreshButtonState();
        }

        private void OnShowSignupClicked()
        {
            if (_isSubmitting) return;
            authSceneController.ShowSignup();
        }

        private void OnLoginClicked()
        {
            if (_isSubmitting) return;
            StartCoroutine(LoginRoutine());
        }

        private IEnumerator LoginRoutine()
        {
            _isSubmitting = true;
            RefreshButtonState();
            authSceneController.SetStatus(string.Empty);
            
            successPopup?.SetActive(false);
            failPopup?.SetActive(false);

            string username = usernameInputField.text.Trim();
            string password = passwordInputField.text;

            AuthTokensResponse tokens = null;
            string errorMessage = null;

            yield return _authClient.Login(
                username,
                password,
                response => tokens = response,
                error => errorMessage = error);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                authSceneController.SetStatus(errorMessage);

                failPopup?.SetActive(true);
                StartCoroutine(HidePopupAfterDelay(failPopup));
                
                _isSubmitting = false;
                RefreshButtonState();
                yield break;
            }

            yield return authSceneController.CompleteAuthentication(
                tokens,
                () =>
                {
                    failPopup?.SetActive(false);
                    successPopup?.SetActive(true);
                    StartCoroutine(HidePopupAfterDelay(successPopup));
                },
                error =>
                {
                    failPopup?.SetActive(true);
                    StartCoroutine(HidePopupAfterDelay(failPopup));
                },
                popupDuration);

            _isSubmitting = false;
            RefreshButtonState();
        }
        
        private IEnumerator HidePopupAfterDelay(GameObject popup)
        {
            yield return new WaitForSeconds(popupDuration);
            popup?.SetActive(false);
        }

        private void RefreshButtonState()
        {
            if (loginButton == null) return;

            bool hasUsername = usernameInputField != null && !string.IsNullOrWhiteSpace(usernameInputField.text);
            bool hasPassword = passwordInputField != null && !string.IsNullOrWhiteSpace(passwordInputField.text);

            loginButton.interactable = !_isSubmitting && hasUsername && hasPassword;
        }
    }
}