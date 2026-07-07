using System;
using System.Collections;
using App;
using Auth;
using Clients;
using Clients.Interfaces;
using Dto.Auth;
using Dto.Profile;
using Services.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UIScripts.Auth
{
    public class AuthSceneController : MonoBehaviour
    {
        [SerializeField] private GameObject loginForm;
        [SerializeField] private GameObject signupForm;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        private AuthSession _authSession;
        private ISessionStorage _sessionStorage;
        private IProfileClient _profileClient;
        private IProfileService _profileService;
        private IBadgeService _badgeService;

        private void Start()
        {
            _authSession = ServiceContainer.Resolve<AuthSession>();
            _sessionStorage = ServiceContainer.Resolve<ISessionStorage>();
            _profileClient = ServiceContainer.Resolve<IProfileClient>();
            _profileService = ServiceContainer.Resolve<IProfileService>();
            _badgeService = ServiceContainer.Resolve<IBadgeService>();

            ShowLogin();
            SetStatus(string.Empty);
        }

        public void ShowLogin()
        {
            if (loginForm != null) loginForm.SetActive(true);
            if (signupForm != null) signupForm.SetActive(false);
            SetStatus(string.Empty);
        }

        public void ShowSignup()
        {
            if (loginForm != null) loginForm.SetActive(false);
            if (signupForm != null) signupForm.SetActive(true);
            SetStatus(string.Empty);
        }

        public IEnumerator CompleteAuthentication(AuthTokensResponse tokens, Action onSuccess = null, Action<string> onError = null, float loadDelaySeconds = 0f)
        {
            if (tokens == null)
            {
                SetStatus("Authentication response is empty.");
                onError?.Invoke("Authentication response is empty.");
                yield break;
            }

            _authSession.ApplyTokens(new AuthTokensResponse
            {
                AccessToken = tokens.AccessToken,
                RefreshToken = tokens.RefreshToken
            });

            _sessionStorage.SaveRefreshToken(tokens.RefreshToken);

            ProfileAwardResponse loginResponse = null;
            string profileError = null;

            yield return _profileClient.DailyLogin(
                response => loginResponse = response,
                error => profileError = error);

            if (!string.IsNullOrWhiteSpace(profileError))
            {
                SetStatus(profileError);
                onError?.Invoke(profileError);
                yield break;
            }

            _profileService.SetLoadedProfile(ProfileMapper.ToProfileData(loginResponse.Profile));
            
            _badgeService.EnqueueAwardedBadges(loginResponse.AwardedBadgeIds);
            onSuccess?.Invoke();

            if (loadDelaySeconds > 0f)
            {
                yield return new WaitForSeconds(loadDelaySeconds);
            }
            
            SceneManager.LoadScene(mainMenuSceneName);
        }

        public void SetStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message ?? string.Empty;
            }
        }
    }
}