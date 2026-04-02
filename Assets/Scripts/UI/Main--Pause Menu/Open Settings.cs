using UnityEngine;

public class OpenSettings : MonoBehaviour
{
    public GameObject settingsFolder;
    public GameObject mainFolder;
    
    public void openSettings()
    {
        settingsFolder.SetActive(true);
        mainFolder.SetActive(false);
    }

    public void closeSettings()
    {
        settingsFolder.SetActive(false);
        mainFolder.SetActive(true);
    }
}
