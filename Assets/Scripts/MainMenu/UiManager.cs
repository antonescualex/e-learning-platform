using System;
using MainMenu;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    [SerializeField] private SettingsService settingsService;

    [SerializeField] private LevelBarService _levelBarService;
    
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text levelText;

    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject mainMenu;
    // [SerializeField] private GameObject optionsMenu;
    // [SerializeField] private CanvasGroup settingsButton;
    [SerializeField] private GameObject settingsPopup;

    private void Update()
    {
        UpdateNameText();
        UpdateCoinsText();
        UpdateLevelText();
        UpdateLevelBar();
    }

    // public void OnPlayButtonPress()
    // {
    //     SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    // }
    
    // public void OnStoreButtonPress()
    // {
    //     SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 2);
    // }
    

    // public void OnQuitButtonPress()
    // {
    //     Application.Quit();
    // }

    private void UpdateCoinsText()
    {
        if (App.Instance.ProfileService != null)
        {
            coinsText.text = App.Instance.ProfileService.ProfileData.Coins.ToString();
        }
    }

    private void UpdateNameText()
    {
        if (App.Instance.ProfileService != null)
        {
            nameText.text = App.Instance.ProfileService.ProfileData.PlayerName.ToString();
        }
    }

    private void UpdateLevelText()
    {
        if (App.Instance.ProfileService != null)
        {
            levelText.text = "Level " + App.Instance.ProfileService.ProfileData.Level.ToString();
        }
    }

    private void UpdateLevelBar()
    {
        if (App.Instance.ProfileService != null)
        {
            _levelBarService.SetProgress(App.Instance.ProfileService.ProfileData.CurrentExperience, App.Instance.ProfileService.ProfileData.ExperienceToNextLevel);
        }
    }

    public void OnSettingsButtonPressed()
    {
        mainMenu.SetActive(false);

        settingsService.OpenSettings();
    }
    
    // public void OnSettingsButtonClick()
    // {
    //     mainMenu.SetActive(false);
    //     optionsMenu.SetActive(true);
    //     HideSettingsButton();
    // }
    //
    // public void OnBackButtonClick()
    // {
    //     mainMenu.SetActive(true);
    //     optionsMenu.SetActive(false);
    //     ShowSettingsButton();
    // }
    //
    // private void HideSettingsButton()
    // {
    //     settingsButton.alpha = 0f;
    //     settingsButton.interactable = false;
    //     settingsButton.blocksRaycasts = false;
    // }
    //
    // private void ShowSettingsButton()
    // {
    //     settingsButton.alpha = 1f;
    //     settingsButton.interactable = true;
    //     settingsButton.blocksRaycasts = true;
    // }
}
