using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Quit : MonoBehaviour
{
    public void quitGame()
    {
        #if UNITY_EDITOR
            // Stop playing the scene in the editor
            EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
        Debug.Log("quit game");
    }
}
