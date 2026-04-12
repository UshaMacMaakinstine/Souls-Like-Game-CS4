using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;


public class SensitivityChange : MonoBehaviour
{

    public Slider slider;
    public int sens;
    public TMP_InputField input;
    public bool sensChange;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        input.text = sens + "";
        slider.value = sens;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void inputChange()
    {
        string sensStr = input.text;
        sens = int.Parse(sensStr);
        slider.value = sens;
        sensChange = true;
    }

    public void sliderChange()
    {
        sens = (int)slider.value;
        input.text = sens + "";
        sensChange = true;
    }
}
