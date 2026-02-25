using System;
using Bootstrap;
using UnityEngine;
using UnityEngine.SceneManagement;

public class App : MonoBehaviour
{
    private static readonly float LOADING_TIME = 1.5f;
    
    public static App Instance { get; private set; }
    
    public ProfileService ProfileService { get; private set; }

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
    }

    private void Start()
    {
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
        AudioManager.Instance.PlayMusic(AudioManager.MusicTypes.Background);
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
