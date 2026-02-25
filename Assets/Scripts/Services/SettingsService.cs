using UnityEngine;

public class SettingsService : MonoBehaviour
{
    [SerializeField] private GameObject settingsPopup;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private Canvas canvas;

    private GameObject currentPopup;

    public void OpenSettings()
    {
        if (currentPopup != null)
        {
            return;
        }
        
        mainMenu.SetActive(false);
        
        currentPopup = Instantiate(settingsPopup, canvas.transform);
        currentPopup.transform.SetAsLastSibling();

        var popup = currentPopup.GetComponent<SettingsPopup>();
        popup.Init(this);
        currentPopup.GetComponent<Ricimi.Popup>()?.Open();
    }

    public void SaveSettings()
    {
        // TODO: De facut logica de save la setari.
        
        CloseSettings();
    }

    public void CancelSettings()
    {
        CloseSettings();
    }

    private void CloseSettings()
    {
        mainMenu.SetActive(true);
        if (currentPopup != null)
        {
            currentPopup.GetComponent<Ricimi.Popup>()?.Close();
            currentPopup = null;
        }
    }
}
