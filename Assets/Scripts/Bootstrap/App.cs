using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class App : MonoBehaviourSingleton<App>
{
    private static readonly float LOADING_TIME = 1.5F;
    
    public ProfileService Profile;

    public override void Awake()
    {
        base.Awake();
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
        AudioManager.Instance.PlayMusic(AudioManager.MusicTypes.Background);
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
