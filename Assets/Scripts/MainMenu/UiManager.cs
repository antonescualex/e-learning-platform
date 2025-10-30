using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    [SerializeField] private TMP_Text coinsText;

    private void Update()
    {
        UpdateCoinsText();
    }

    public void OnPlayButtonPress()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    
    public void OnStoreButtonPress()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 2);
    }
    

    public void OnQuitButtonPress()
    {
        Application.Quit();
    }

    private void UpdateCoinsText()
    {
        if (App.Instance.Profile != null)
        {
            coinsText.text = App.Instance.Profile.Data.Coins.ToString();
        }
    }
}
