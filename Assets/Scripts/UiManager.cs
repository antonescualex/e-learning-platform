using UnityEngine;
using UnityEngine.SceneManagement;

public class UiManager : MonoBehaviour
{
    
    
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

    public void OnVolumeSliderChanged(float value)
    {
        
    }
}
