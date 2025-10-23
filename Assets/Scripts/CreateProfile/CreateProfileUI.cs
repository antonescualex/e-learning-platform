using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CreateProfileUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField playerNameInputField;
    [SerializeField] private Button enterButton;

    private void Awake()
    {
        enterButton.onClick.AddListener(OnEnterClicked);
        enterButton.interactable = false;
        playerNameInputField.onValueChanged.AddListener(OnInputChanged);
    }

    private void OnInputChanged(string newText)
    {
        enterButton.interactable = !string.IsNullOrWhiteSpace(newText);
    }

    private void OnEnterClicked()
    {
        App.Instance.Profile.CreateNewProfile(playerNameInputField.text.Trim());
        SceneManager.LoadScene("MainMenu");
    }
}
