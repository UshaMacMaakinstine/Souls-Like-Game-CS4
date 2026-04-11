using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChangeKeybind : MonoBehaviour
{
    public TMP_Text keyText;
    public GameObject excapeText;
    public bool changingKey;

    public PlayerInput playerInput;

    // Update is called once per frame
    void Update()
    {
        if(changingKey)
        {
            string inp = Input.inputString;
            if(inp != "")
            {
                keyText.text = inp[0] +"";
                excapeText.SetActive(false);
                //change the keybind
                changingKey=false;
            }
        }
        
    }

    public void OnClick()
    {
        keyText.text = "";
        excapeText.SetActive(true);
        changingKey = true;
    }


}
