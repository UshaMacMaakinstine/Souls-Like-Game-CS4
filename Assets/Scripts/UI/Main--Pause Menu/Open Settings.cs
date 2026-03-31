using UnityEngine;

public class OpenSettings : MonoBehaviour
{
    public GameObject settingsFolder;
    public GameObject mainFolder;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

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
