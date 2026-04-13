using UnityEngine;
using UnityEngine.InputSystem;

public class RebindPersistence : MonoBehaviour
{
    public static RebindPersistence Instance; // Singleton reference
    public InputActionAsset actions;

    void Awake()
    {
        // Singleton pattern to prevent duplicates across scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scene loads
            LoadRebinds();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDisable() => SaveRebinds();

    public void SaveRebinds()
    {
        string rebinds = actions.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString("rebinds", rebinds);
        PlayerPrefs.Save();
    }

    public void LoadRebinds()
    {
        string rebinds = PlayerPrefs.GetString("rebinds");
        if (!string.IsNullOrEmpty(rebinds))
            actions.LoadBindingOverridesFromJson(rebinds);
    }
}
