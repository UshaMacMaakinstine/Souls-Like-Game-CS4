using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonLoadScene : MonoBehaviour
{
    public string scene;
    
    public void loadScene()
    {
        SceneManager.LoadScene(scene);
    }
}
