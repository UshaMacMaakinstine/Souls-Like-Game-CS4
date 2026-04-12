using UnityEngine;
using UnityEngine.InputSystem;

public class InputSaveManager : MonoBehaviour
{
    public InputActionAsset inputActions;
    private const string SaveKey = "CustomKeybinds";

    private void Awake()
    {
        // 1. Load the saved binds as soon as the game starts
        LoadBinds();
    }

    public void SaveBinds()
    {
        // 2. Convert all current overrides into a single JSON string
        string overrides = inputActions.SaveBindingOverridesAsJson();
        
        // 3. Save that string to PlayerPrefs
        PlayerPrefs.SetString(SaveKey, overrides);
        PlayerPrefs.Save();
        
        Debug.Log("Keybinds Saved!");
    }

    public void LoadBinds()
    {
        if (PlayerPrefs.HasKey(SaveKey))
        {
            string overrides = PlayerPrefs.GetString(SaveKey);
            
            // 4. Apply the JSON back to the Action Asset
            inputActions.LoadBindingOverridesFromJson(overrides);
            Debug.Log("Keybinds Loaded!");
        }
    }

    // Optional: Add a button in your UI to call this
    public void ResetBinds()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        foreach (var map in inputActions.actionMaps)
        {
            map.RemoveAllBindingOverrides();
        }
    }
}