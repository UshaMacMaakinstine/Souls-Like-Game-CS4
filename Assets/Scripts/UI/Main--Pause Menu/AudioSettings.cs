using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class AudioSettings : MonoBehaviour
{
    public Slider slider;
    public int value;
    public TMP_InputField input;
    public bool AudioChange;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        input.text = value + "";
        slider.value = value;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void inputChange()
    {
        string sensStr = input.text;
        value = int.Parse(sensStr);
        slider.value = value;
        AudioChange = true;
    }

    public void sliderChange()
    {
        value = (int)slider.value;
        input.text = value + "";
        AudioChange = true;
    }
}
