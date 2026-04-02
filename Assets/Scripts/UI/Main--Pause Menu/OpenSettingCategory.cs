using UnityEngine;

public class OpenSettingCategory : MonoBehaviour
{
    public GameObject specificSettingsFolder;
    public GameObject[] otherSettingsFolder;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void onClick()
    {
        specificSettingsFolder.SetActive(true);
        for(int i = 0; i < otherSettingsFolder.Length;i++)
        {
            otherSettingsFolder[i].SetActive(false);
        }
    }
    
}
