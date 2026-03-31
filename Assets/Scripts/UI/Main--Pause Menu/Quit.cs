using UnityEngine;

public class Quit : MonoBehaviour
{
    public void quitGame()
    {
        Application.Quit();
        UnityEditor.EditorApplication.isPlaying = false;
        Debug.Log("quit game");
    }
}
