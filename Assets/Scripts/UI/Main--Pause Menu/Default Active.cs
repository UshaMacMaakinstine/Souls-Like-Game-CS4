using UnityEngine;

public class DefaultActive : MonoBehaviour
{
    public bool useDefault;
    public GameObject mainFolder;
    public GameObject settingsFolder;
    public GameObject GameSet;
    public GameObject AudioSet;
    public GameObject KeybindSet;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(useDefault)
        {
            GameSet.SetActive(true);
            AudioSet.SetActive(false);
            KeybindSet.SetActive(false);
            settingsFolder.SetActive(false);
            mainFolder.SetActive(true);
        }
    }
}
