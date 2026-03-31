using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class ProfileScreenScript : MonoBehaviour
{
    public GameObject profilePanel;
    public GameObject settingsPanel;
    
    // text elements
    public TextMeshProUGUI usernameText;
    public TextMeshProUGUI plushyNameText;
    public TextMeshProUGUI sanityText;
    public TextMeshProUGUI careCyclesText;
    public TextMeshProUGUI musicVolumeText;
    public TextMeshProUGUI sfxStatusText;

    // buttons
    public Button closePButton;
    public Button closeSButton;
    public Button plushyButton;
    public Button settingsButton;
    public Button quitButton;

    // sound
    public Slider musicSlider;
    public Toggle sfxToggle;
    
    // references
    private SanityManagerScript sanityManager;
    private PlushyScript plushyScript;
    
    void Start()
    {
        sanityManager = FindObjectOfType<SanityManagerScript>();
        plushyScript = FindObjectOfType<PlushyScript>();
        
        // hide panels at start
        if (profilePanel != null)
        {
            profilePanel.SetActive(false);
        }
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        // listen on button clicks
        if (closePButton != null)
        {
            closePButton.onClick.AddListener(onCloseButtonClicked);
        }
        if (closeSButton != null)
        {
            closeSButton.onClick.AddListener(onCloseButtonClicked);
        }

        if (plushyButton != null)
        {
            plushyButton.onClick.AddListener(onPlushyButtonClicked);
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(onSettingsButtonClicked);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(onQuitButtonClicked);
        }

        // slider listeners
        if (musicSlider != null)
        {
            musicSlider.onValueChanged.AddListener(onMusicVolumeChanged);
        }

        // toggle listener
        if (sfxToggle != null)
        {
            sfxToggle.onValueChanged.AddListener(onSFXToggleChanged);
        }
        
        // load saved volumes
        loadVolumeSettings();
    }
    
    void onCloseButtonClicked()
    {
        // button click
        if (AudioManager.instance != null)
        {
            AudioManager.instance.playButtonClick();
        }

        hideProfile();
        hideSettings();
    }
    void onPlushyButtonClicked()
    {
        // button click
        if (AudioManager.instance != null)
        {
            AudioManager.instance.playButtonClick();
        }

        if(plushyScript.gameStarted)
        {
            showProfile();
        }
    }
    void onSettingsButtonClicked()
    {
        // button click
        if (AudioManager.instance != null)
        {
            AudioManager.instance.playButtonClick();
        }

        if(plushyScript.gameStarted)
        {
            showSettings();
        }
    }
    void onQuitButtonClicked()
    {
        // button click
        if (AudioManager.instance != null)
        {
            AudioManager.instance.playButtonClick();
        }
        
        plushyScript.gameStarted = false;
        sanityManager.saveGame();
        // disable button
        quitButton.interactable = false;
        SceneManager.LoadSceneAsync("StartMenu");
    }

    // profile
    public void showProfile()
    {
        // load and display user data
        loadProfileData();
        
        // show panel
        if (profilePanel != null)
        {
            profilePanel.SetActive(true);
        }
    }
    
    public void hideProfile()
    {
        if (profilePanel != null)
        {
            profilePanel.SetActive(false);
        }
    }

    // settings
    public void showSettings()
    {
        // load and display settings data
        loadSettingsData();
        plushyScript.gameStarted = false;
        
        // show panel
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    public void hideSettings()
    {
        plushyScript.gameStarted = true;
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }
    
    void loadProfileData()
    {
        // plushy name
        string plushyName = SaveManager.loadString("PlushyName", "Unknown");
        if (plushyNameText != null)
        {
            plushyNameText.text = "Plushy: " + plushyName;
        }
        
        // current sanity
        if (sanityManager != null && sanityText != null)
        {
            sanityText.text = "Sanity: " + sanityManager.currentSanity + " / " + sanityManager.maxSanity;
        }
        
        // care cycles
        int careCycles = SaveManager.loadInt("CareCycles", 0);
        if (careCyclesText != null)
        {
            careCyclesText.text = "Care Cycles: " + careCycles;
        }
    }

    void loadSettingsData()
    {
        // username
        string username = SaveManager.getCurrentUser();
        if (usernameText != null)
        {
            usernameText.text = "Player: " + username;
        }
    }

    void loadVolumeSettings()
    {
        // load saved music volume (default: 0.3)
        float musicVol = PlayerPrefs.GetFloat("MusicVolume", 0.3f);
        
        // load saved SFX state (default: ON = 1)
        int sfxEnabled = PlayerPrefs.GetInt("SFXEnabled", 1);
        bool isSFXOn = (sfxEnabled == 1);
        
        // set slider value
        if (musicSlider != null)
        {
            musicSlider.value = musicVol;
        }
        
        // set toggle value
        if (sfxToggle != null)
        {
            sfxToggle.isOn = isSFXOn;
        }
        
        // update text displays
        updateVolumeText();
        updateSFXText();
        
        // apply to AudioManager
        if (AudioManager.instance != null)
        {
            AudioManager.instance.setMusicVolume(musicVol);
            AudioManager.instance.setSFXVolume(isSFXOn ? 1.0f : 0f);
        }
    }
    
    void onMusicVolumeChanged(float volume)
    {
        // save volume
        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();
        
        // update display
        updateVolumeText();
        
        // apply to AudioManager
        if (AudioManager.instance != null)
        {
            AudioManager.instance.setMusicVolume(volume);
        }
    }
    
    void onSFXToggleChanged(bool isOn)
    {
        // save state (1 = on, 0 = off)
        PlayerPrefs.SetInt("SFXEnabled", isOn ? 1 : 0);
        PlayerPrefs.Save();
        
        // update display
        updateSFXText();
        
        // apply to AudioManager
        if (AudioManager.instance != null)
        {
            AudioManager.instance.setSFXVolume(isOn ? 1.0f : 0f);
        }
        
        // play test sound if turning ON
        if (isOn && AudioManager.instance != null)
        {
            AudioManager.instance.playButtonClick();
        }
    }
    
    void updateVolumeText()
    {
        if (musicSlider != null && musicVolumeText != null)
        {
            int percentage = Mathf.RoundToInt(musicSlider.value * 100);
            musicVolumeText.text = percentage + "%";
        }
    }

    void updateSFXText()
    {
        if (sfxToggle != null && sfxStatusText != null)
        {
            sfxStatusText.text = sfxToggle.isOn ? "ON" : "OFF";
        }
    }

}