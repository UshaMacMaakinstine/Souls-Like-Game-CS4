using UnityEngine;

public class DefaultActive : MonoBehaviour
{
    public GameObject mainFolder;
    public GameObject settingsFolder;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        settingsFolder.SetActive(false);
        mainFolder.SetActive(true);
    }
}
