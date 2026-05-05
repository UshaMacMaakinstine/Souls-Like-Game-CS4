using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.IO;
using Cinemachine;



#if UNITY_EDITOR
using UnityEditor;
#endif

public class UIController : MonoBehaviour
{
    [Header("Sens")]
    public int sens;
    public Slider sensSlider;
    public TMP_InputField sensInput;
    [SerializeField] public CinemachineFreeLook freeLook;

    [Header("Main Audio")]
    public int mainAudio;
    public Slider mainAudioSlider;
    public TMP_InputField mainAudioInput;

    [Header("Music Audio")]
    public int musicAudio;
    public Slider musicAudioSlider;
    public TMP_InputField musicAudioInput;

    [Header("SFX Audio")]
    public int sfxAudio;
    public Slider sfxAudioSlider;
    public TMP_InputField sfxAudioInput;

    [Header("Voice Lines Audio")]
    public int voiceAudio;
    public Slider voiceAudioSlider;
    public TMP_InputField voiceAudioInput;

    [Header("Settings")]
    public GameObject[] settingsTab;

    [Header("Page Next/Prev")]
    int pageNumber = 0;
    public GameObject[] pages;
    public Button PreviousButton;
    public Button NextButton;

    [Header("Open/Close Settings")]
    public GameObject settingsFolder;
    public GameObject mainFolder;
    public Button GameButton;

    string path;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        path = Path.Combine(Application.persistentDataPath, "settingsData.json");
        
        // 1. Load the data first
        GameData data = LoadData();

        sensSlider.value = data.savedSens;
        mainAudioSlider.value = data.savedMainAudio;
        musicAudioSlider.value = data.savedMusicAudio;
        sfxAudioSlider.value = data.savedSFXAudio;
        voiceAudioSlider.value = data.savedVoiceLinesAudio;
    }
    
    void Update()
    {
        if(pageNumber >= (pages.Length - 1))
            NextButton.GetComponent<Button>().interactable = false;
        else
            NextButton.GetComponent<Button>().interactable = true;
        if(pageNumber <= 0)
            PreviousButton.GetComponent<Button>().interactable = false;
        else
            PreviousButton.GetComponent<Button>().interactable = true;
    }

        // --- Sensitivity ---
    public void sensInputChange()
    {
        if (int.TryParse(sensInput.text, out int result)) {
            sens = result;
            if (sensSlider.value != sens) sensSlider.value = sens;
        }
    }
    public void sensSliderChange() 
    {
        sens = (int)sensSlider.value;
        if (sensInput.text != sens.ToString()) sensInput.text = sens.ToString();

        freeLook.m_XAxis.m_MaxSpeed = MapValue(sens, 0f, 1f, 0f, 0.8f);
        freeLook.m_YAxis.m_MaxSpeed = MapValue(sens, 0f, 1f, 0f, 0.016f);
    }

    private float MapValue(float val, float srcMin, float srcMax, float dstMin, float dstMax)
    {
        return ((val - srcMin) / (srcMax - srcMin) * (dstMax - dstMin) + dstMin) / 100f;
    }

    // --- Main Audio ---
    public void mainAudioInputChange() 
    {
        if (int.TryParse(mainAudioInput.text, out int result)) {
            mainAudio = result;
            if (mainAudioSlider.value != mainAudio) mainAudioSlider.value = mainAudio;
        }
    }
    public void mainAudioSliderChange() 
    {
        mainAudio = (int)mainAudioSlider.value;
        if (mainAudioInput.text != mainAudio.ToString()) mainAudioInput.text = mainAudio.ToString();
    }

    // --- Music Audio ---
    public void musicAudioInputChange() 
    {
        if (int.TryParse(musicAudioInput.text, out int result)) {
            musicAudio = result;
            if (musicAudioSlider.value != musicAudio) musicAudioSlider.value = musicAudio;
        }
    }
    public void musicAudioSliderChange() 
    {
        musicAudio = (int)musicAudioSlider.value;
        if (musicAudioInput.text != musicAudio.ToString()) musicAudioInput.text = musicAudio.ToString();
    }

    // --- SFX Audio ---
    public void sfxInputChange() 
    {
        if (int.TryParse(sfxAudioInput.text, out int result)) {
            sfxAudio = result;
            if (sfxAudioSlider.value != sfxAudio) sfxAudioSlider.value = sfxAudio;
        }
    }
    public void sfxSliderChange() 
    {
        sfxAudio = (int)sfxAudioSlider.value;
        if (sfxAudioInput.text != sfxAudio.ToString()) sfxAudioInput.text = sfxAudio.ToString();
    }

    // --- Voice Audio ---
    public void voiceInputChange() 
    {
        if (int.TryParse(voiceAudioInput.text, out int result)) {
            voiceAudio = result;
            if (voiceAudioSlider.value != voiceAudio) voiceAudioSlider.value = voiceAudio;
        }
    }
    public void voiceSliderChange() 
    {
        voiceAudio = (int)voiceAudioSlider.value;
        if (voiceAudioInput.text != voiceAudio.ToString()) voiceAudioInput.text = voiceAudio.ToString();
    }

    public void quitGame()
    {
        #if UNITY_EDITOR
            // Stop playing the scene in the editor
            EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
        SaveData( new GameData
        {
            savedSens = sens,
            savedMainAudio = mainAudio,
            savedMusicAudio = musicAudio,
            savedSFXAudio = sfxAudio,
            savedVoiceLinesAudio = voiceAudio
        });
        Debug.Log("quit game");
    }

    public void resume()
    {
        mainFolder.SetActive(false);
        settingsFolder.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public void SettingsTabChange(int i)
    {
        foreach(GameObject tab in settingsTab)
            tab.SetActive(false);
        settingsTab[i].SetActive(true);
    }

    public void PageNext()
    {
        pages[pageNumber].SetActive(false);
        pages[pageNumber + 1].SetActive(true);
        pageNumber++;
    }

    public void PagePrev()
    {
        Debug.Log("Click");
        pages[pageNumber].SetActive(false);
        pages[pageNumber - 1].SetActive(true);
        pageNumber--;
    }

    public void LoadScene(string Scene)
    {
        SceneManager.LoadScene(Scene);
    }

    public void openSettings()
    {
        settingsFolder.SetActive(true);
        GameButton.Select();
        mainFolder.SetActive(false);
    }

    public void closeSettings()
    {
        settingsFolder.SetActive(false);
        mainFolder.SetActive(true);
    }

    public void SaveData(GameData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
    }

    public GameData LoadData()
    {
        if (File.Exists(path)) {
            string json = File.ReadAllText(path);
            Debug.Log("Loaded tile data from file: " + path);
            return JsonUtility.FromJson<GameData>(json);
        }
        return new GameData();
    }
}
