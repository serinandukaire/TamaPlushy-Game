using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PlushyNameScript : MonoBehaviour
{
    public static PlushyNameScript instance; // singleton
    public GameObject sanityPanel;
    
    public TextMeshProUGUI savedPlushyText;
    public TextMeshProUGUI errorText;
    public GameObject plushyNamingPanel;
    public TMP_InputField plushyNameInput;
    public Button startButton;
    
    public bool namingComplete = false; // flag
    
    void Awake()
    {
        // singleton
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        if (startButton != null)
        {
            startButton.onClick.AddListener(onStartButtonClicked);
        }
        
        // hide plushy naming panel at start
        if (plushyNamingPanel != null)
        {
            plushyNamingPanel.SetActive(false);
        }
    }
    
    // when user clicks start after naming plushy
    void onStartButtonClicked()
    {
        // button click
        if (AudioManager.instance != null)
        {
            AudioManager.instance.playButtonClick();
        }
        
        string plushyName = plushyNameInput.text.Trim();
        
        // validate input
        if (string.IsNullOrEmpty(plushyName))
        {
            showError("Please Name Your Plushy!");
            return;
        }
        else
        {
            if (savedPlushyText != null)
            {
                savedPlushyText.text = "Creating plushy...";
                savedPlushyText.gameObject.SetActive(true);
            }
        }
        
        SaveManager.saveString("PlushyName", plushyName);
        
        // disable button
        startButton.interactable = false;
        
        // load game (hide panel)
        StartCoroutine(loadGameWithDelay(1.5f));
    }
    
    public void showPanel()
    {
        // hide sanity panel
        if (sanityPanel != null)
        {
            sanityPanel.SetActive(false);
        }

        // show panel
        if (plushyNamingPanel != null)
        {
            plushyNamingPanel.SetActive(true);
        }
    }
    
    void showError(string message)
    {
        if (errorText != null)
        {
            errorText.text = message;
            errorText.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning(message);
        }
    }
    
    // add delay before loading
    IEnumerator loadGameWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // show sanity panel
        if (sanityPanel != null)
        {
            sanityPanel.SetActive(true);
        }

        // hide panel
        if (plushyNamingPanel != null)
        {
            plushyNamingPanel.SetActive(false);
        }

        // set flag
        namingComplete = true;
    }
}