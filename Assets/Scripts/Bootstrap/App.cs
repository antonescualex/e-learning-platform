using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class App : MonoBehaviour
{
    private static readonly float LOADING_TIME = 1.5F;
    
    public static App Instance { get; private set; }
    
    public ProfileService Profile;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Profile = new ProfileService();
    }

    private void Start()
    {
        bool hasProfile = Profile.TryLoadProfile();
        if (hasProfile)
        {
            Debug.Log($"Loaded existing profile: {Profile.Data.PlayerName}, Level: {Profile.Data.Level}, Coins: {Profile.Data.Coins}");
            Invoke(nameof(LoadMainMenuScene), LOADING_TIME);
        }
        else
        {
            Invoke(nameof(LoadCreateProfileScene), LOADING_TIME);
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
        Profile?.SaveProfile();
    }
}
