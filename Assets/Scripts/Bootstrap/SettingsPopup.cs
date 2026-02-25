using UnityEngine;
using UnityEngine.UI;

public class SettingsPopup : MonoBehaviour
{
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider soundVolumeSlider;
    [SerializeField] private Slider musicToggle;
    [SerializeField] private Slider soundToggle;

    private SettingsService _settingsService;

    public void Init(SettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public void OnSave()
    {
        // TODO: Implementare logica save
        _settingsService.SaveSettings();
    }

    public void OnCancel()
    {
        _settingsService.CancelSettings();
    }
}
