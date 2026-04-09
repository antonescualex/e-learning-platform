using Data;
using Data.StaticData.Accessory;
using Data.StaticData.Item;
using Repositories;
using Services;
using Services.Interfaces;
using Storage;
using UIScripts.Bootstrap;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace App
{
    public class App : MonoBehaviour
    {
        private static readonly float LOADING_TIME = 1.5f;

        [SerializeField] private BadgeCatalog badgeCatalog;
        [SerializeField] private BoosterCatalog boosterCatalog;
        [SerializeField] private RewardCatalog rewardCatalog;
        [SerializeField] private AccessoryCatalog accessoryCatalog;
        [SerializeField] private string lessonContentBaseUrl = global::Services.LessonContentService.DefaultBaseUrl;

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

            IStorage storage = new JsonStorage("E-LearningApp");
            ServiceContainer.Register<IStorage>(storage);

            var profileRepository = new ProfileRepository(storage);
            ServiceContainer.Register<IRepository<ProfileData>>(profileRepository);

            var settingsRepository = new SettingsRepository(storage);
            ServiceContainer.Register<IRepository<SettingsData>>(settingsRepository);

            var profileService = new ProfileService(profileRepository);
            ServiceContainer.Register<IProfileService>(profileService);

            var badgeService = new BadgeService(badgeCatalog, profileService);
            ServiceContainer.Register<IBadgeService>(badgeService);

            var settingsService = new SettingsService(settingsRepository);
            settingsService.LoadOrDefault();
            ServiceContainer.Register<ISettingsService>(settingsService);

            ServiceContainer.Register<IProfileItemsService>(
                new ProfileItemsService(badgeCatalog, boosterCatalog, rewardCatalog, profileService));
            ServiceContainer.Register<ILessonService>(new LessonService(profileService, boosterCatalog, rewardCatalog, badgeService));
            ServiceContainer.Register<ILessonContentService>(new LessonContentService(lessonContentBaseUrl));
            ServiceContainer.Register<IInventoryService>(new InventoryService(accessoryCatalog, profileService));
            ServiceContainer.Register<IShopService>(new ShopService(accessoryCatalog, profileService, badgeService));
        }

        private void Start()
        {
            ISettingsService settingsService = ServiceContainer.Resolve<ISettingsService>();
            IProfileService profileService = ServiceContainer.Resolve<IProfileService>();
            IBadgeService badgeService = ServiceContainer.Resolve<IBadgeService>();

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.ApplySettings(settingsService.CurrentSettings);
            }

            if (profileService.TryLoadProfile())
            {
                badgeService.HandleAppOpened();
                Invoke("LoadMainMenuScene", LOADING_TIME);
            }
            else
            {
                Invoke("LoadCreateProfileScene", LOADING_TIME);
            }
        }

        private void LoadMainMenuScene()
        {
            SceneManager.LoadScene("MainMenu");
        }

        private void LoadCreateProfileScene()
        {
            SceneManager.LoadScene("CreateProfile");
        }

        private void OnApplicationQuit()
        {
            if (ServiceContainer.TryResolve<IProfileService>(out IProfileService profileService))
            {
                profileService.SaveProfile();
            }
        }
    }
}
