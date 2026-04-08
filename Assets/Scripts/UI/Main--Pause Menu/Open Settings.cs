using UnityEngine;
using UnityEngine.UI;

public class OpenSettings : MonoBehaviour
{
    public GameObject settingsFolder;
    public GameObject mainFolder;
    public Button GameButton;
    
    public void openSettings()
    {
        settingsFolder.SetActive(true);
        GameButton.Select();
        mainFolder.SetActive(false);
    }

    public void closeSettings()
    {
        settingsFolder.SetActive(false);
        mainFolder.SetActive(true);
    }
}
