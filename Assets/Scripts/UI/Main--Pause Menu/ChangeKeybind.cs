using UnityEngine;
using UnityEngine.UI;

public class ChangeKeybind : MonoBehaviour
{
    //public TMP_Text keyText;
    public GameObject excapeText;
    public bool changingKey;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClick()
    {
        //keyText.text = "";
        excapeText.SetActive(true);
        changingKey = true;
    }
}
