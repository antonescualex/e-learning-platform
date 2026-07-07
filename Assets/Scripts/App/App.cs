using System.Collections;
using Auth;
using Clients;
using Clients.Interfaces;
using Data;
using Data.StaticData.Background;
using Data.StaticData.Badge;
using Data.StaticData.Booster;
using Data.StaticData.Item;
using Data.StaticData.Shop;
using Dto;
using Dto.Auth;
using Dto.Profile;
using Repositories;
using Services;
using Services.Interfaces;
using Storage;
using UIScripts.Bootstrap;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace App
{
    public class App : MonoBehaviour
    {
        private static readonly float LoadingTime = 1.5f;

        [SerializeField] private BadgeCatalog badgeCatalog;
        [SerializeField] private BoosterCatalog boosterCatalog;
        [SerializeField] private BackgroundCatalog backgroundCatalog;
        [FormerlySerializedAs("accessoryCatalog")] [SerializeField] private ShopCatalog shopCatalog;
        [SerializeField] private string lessonContentBaseUrl = LessonContentService.DefaultBaseUrl;

        public static App Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            ServiceContainer.Reset();
            
            var authSession = new AuthSession();
            ServiceContainer.Register(authSession);
            
            IProfileClient profileClient = new ProfileClient(lessonContentBaseUrl, authSession);
            ServiceContainer.Register<IProfileClient>(profileClient);
            
            IBoosterClient boosterClient = new BoosterClient(lessonContentBaseUrl, authSession);
            ServiceContainer.Register<IBoosterClient>(boosterClient);
            
            ILessonClient lessonClient = new LessonClient(lessonContentBaseUrl, authSession);
            ServiceContainer.Register<ILessonClient>(lessonClient);
            
            IShopClient shopClient = new ShopClient(lessonContentBaseUrl, authSession);
            ServiceContainer.Register<IShopClient>(shopClient);

            ISessionStorage sessionStorage = new PlayerPrefsSessionStorage("Learnity.RefreshToken");
            ServiceContainer.Register<ISessionStorage>(sessionStorage);

            IAuthClient authClient = new AuthClient(lessonContentBaseUrl, authSession);
            ServiceContainer.Register<IAuthClient>(authClient);

            IStorage storage = new JsonStorage("E-LearningApp");
            ServiceContainer.Register<IStorage>(storage);

            var settingsRepository = new SettingsRepository(storage);
            ServiceContainer.Register<IRepository<SettingsData>>(settingsRepository);

            var profileService = new ProfileService();
            ServiceContainer.Register<IProfileService>(profileService);

            var badgeService = new BadgeService(badgeCatalog, profileService);
            ServiceContainer.Register<IBadgeService>(badgeService);
            
            var boosterService = new BoosterService(boosterCatalog, profileService, boosterClient);
            ServiceContainer.Register<IBoosterService>(boosterService);

            var settingsService = new SettingsService(settingsRepository);
            settingsService.LoadOrDefault();
            ServiceContainer.Register<ISettingsService>(settingsService);
            ServiceContainer.Register<ILessonService>(new LessonService(profileService, boosterService, badgeService, lessonClient));
            ServiceContainer.Register<ILessonContentService>(new LessonContentService(lessonContentBaseUrl));
            ServiceContainer.Register<IInventoryService>(new InventoryService(backgroundCatalog, profileService));
            ServiceContainer.Register<IShopService>(new ShopService(shopCatalog, profileService, badgeService, shopClient));
        }

        private void Start()
        {
            ISettingsService settingsService = ServiceContainer.Resolve<ISettingsService>();

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.ApplySettings(settingsService.CurrentSettings);
            }
            
            // PlayerPrefs.DeleteKey("Learnity.RefreshToken");
            // PlayerPrefs.Save();
            
            StartCoroutine(BootstrapApplication());
        }
        
        private IEnumerator BootstrapApplication()
        {
            AuthSession authSession = ServiceContainer.Resolve<AuthSession>();
            ISessionStorage sessionStorage = ServiceContainer.Resolve<ISessionStorage>();
            IAuthClient authClient = ServiceContainer.Resolve<IAuthClient>();
            IProfileClient profileClient = ServiceContainer.Resolve<IProfileClient>();
            IProfileService profileService = ServiceContainer.Resolve<IProfileService>();

            if (!sessionStorage.TryLoadRefreshToken(out string storedRefreshToken))
            {
                Invoke(nameof(LoadCreateProfileScene), LoadingTime);
                yield break;
            }

            authSession.SetRefreshToken(storedRefreshToken);

            AuthTokensResponse refreshedTokens = null;
            string refreshError = null;

            yield return authClient.RefreshSession(
                storedRefreshToken,
                tokens => refreshedTokens = tokens,
                error => refreshError = error);

            if (!string.IsNullOrWhiteSpace(refreshError) || refreshedTokens == null)
            {
                Debug.LogWarning("Refresh failed. Clearing local auth session.\n" + refreshError);

                authSession.Clear();
                sessionStorage.ClearRefreshToken();

                Invoke(nameof(LoadCreateProfileScene), LoadingTime);
                yield break;
            }

            authSession.ApplyTokens(refreshedTokens);
            sessionStorage.SaveRefreshToken(refreshedTokens.RefreshToken);

            ProfileAwardResponse loginResponse = null;
            string profileError = null;

            yield return profileClient.DailyLogin(
                response => loginResponse = response,
                error => profileError = error);

            if (!string.IsNullOrWhiteSpace(profileError) || loginResponse == null)
            {
                Debug.LogWarning("Profile bootstrap failed. Clearing local auth session.\n" + profileError);

                authSession.Clear();
                sessionStorage.ClearRefreshToken();

                Invoke(nameof(LoadCreateProfileScene), LoadingTime);
                yield break;
            }

            profileService.SetLoadedProfile(ProfileMapper.ToProfileData(loginResponse.Profile));

            if (ServiceContainer.TryResolve<IBadgeService>(out IBadgeService badgeService))
            {
                badgeService.EnqueueAwardedBadges(loginResponse.AwardedBadgeIds);
            }

            Invoke(nameof(LoadMainMenuScene), LoadingTime);
        }
        
        private void LoadMainMenuScene()
        {
            SceneManager.LoadScene("MainMenu");
        }

        private void LoadCreateProfileScene()
        {
            SceneManager.LoadScene("CreateProfile");
        }
    }
}
