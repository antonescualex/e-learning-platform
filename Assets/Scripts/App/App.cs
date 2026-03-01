using Data.StaticData;
using Repositories;
using Services;
using Storage;
using UIScripts.Bootstrap;
using UnityEngine;
using UnityEngine.SceneManagement;

public class App : MonoBehaviour
{
    private static readonly float LOADING_TIME = 1.5f;

    [SerializeField] private ItemCatalogScriptableObject itemCatalog;
    
    public static App Instance { get; private set; }
    
    public IProfileService ProfileService { get; private set; }
    public ISettingsService SettingsService { get; private set; }
    public IProfileItemsService ProfileItemsService { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        IStorage storage = new JsonStorage("E-LearningApp");

        var profileRepository = new ProfileRepository(storage);
        ProfileService = new ProfileService(profileRepository);
        
        var settingsRepository = new SettingsRepository(storage);
        SettingsService = new SettingsService(settingsRepository);
        SettingsService.LoadOrDefault();

        ProfileItemsService = new ProfileItemsService(itemCatalog);
    }

    private void Start()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ApplySettings(SettingsService.CurrentSettings);
        }
        
        if (ProfileService.TryLoadProfile())
        {
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
        ProfileService?.SaveProfile();
    }
}
