using UnityEngine;
using UnityEngine.InputSystem;

public class RebindSaveLoad : MonoBehaviour
{
    // Assign your .inputactions asset here in the inspector
    [SerializeField] private InputActionAsset actionAsset;
    private const string RebindsKey = "rebinds";

    private void Awake()
    {
        LoadOverrides();
    }

    public void SaveOverrides()
    {
        // Important: We call this to ensure all current overrides are captured
        string rebinds = actionAsset.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString(RebindsKey, rebinds);
        PlayerPrefs.Save();
    }

    public void LoadOverrides()
    {
        string rebinds = PlayerPrefs.GetString(RebindsKey);
        if (!string.IsNullOrEmpty(rebinds))
        {
            // 1. Disable the asset before applying
            actionAsset.Disable();

            // 2. Load the JSON
            actionAsset.LoadBindingOverridesFromJson(rebinds);

            // 3. Re-enable to force the system to see the new paths
            actionAsset.Enable();
        }
    }

    // Optional: Reset to defaults
    public void ResetBinds()
    {
        foreach (InputActionMap map in actionAsset.actionMaps)
        {
            map.RemoveAllBindingOverrides();
        }
        PlayerPrefs.DeleteKey(RebindsKey);
    }
}