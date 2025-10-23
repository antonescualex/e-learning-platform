using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class App : MonoBehaviour
{
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
        Profile.InitializeProfile();
    }

    private void Start()
    {
        Debug.Log($"Loaded existing profile: {Profile.Data.PlayerName}, Level: {Profile.Data.Level}, Coins: {Profile.Data.Coins}");
        Invoke(nameof(LoadMainMenuScene), 2F);
    }

    private void LoadMainMenuScene()
    {
        SceneManager.LoadScene("MainMenu");
    }
    
    private void OnApplicationQuit()
    {
        Profile?.SaveProfile();
    }
}
