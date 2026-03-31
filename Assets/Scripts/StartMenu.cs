using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class StartMenu : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public TextMeshProUGUI errorText;
    public TextMeshProUGUI savedText;
    public Button enterButton;
    public GameObject introPanel;
    public GameObject usernamePanel;
    
    private string currentUsername = "";

    void Update()
    {
        var keyboard = Keyboard.current;
        
        // press o key to wipe all save data
        if (keyboard != null && keyboard.oKey.wasPressedThisFrame)
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("ALL SAVE DATA DELETED!");
        }
    }
    
    void Start()
    {
        // play sound 
        if (AudioManager.instance != null)
        {
            AudioManager.instance.playBackgroundMusic();
        }

        // add button listener
        if (enterButton != null)
        {
            enterButton.onClick.AddListener(onEnterButtonClicked);
        }
        
        // hide username panel at start
        if (usernamePanel != null)
        {
            usernamePanel.SetActive(false);
        }

        // hide saved text
        if (savedText != null)
        {
            savedText.gameObject.SetActive(false);
        }

        // show intro then username panel
        StartCoroutine(loadIntroGameWithDelay(2f));
    }
    
    void onEnterButtonClicked()
    {
        // button click
        if (AudioManager.instance != null)
        {
            AudioManager.instance.playButtonClick();
        }

        currentUsername = usernameInput.text.Trim();
        
        // validate input
        if (string.IsNullOrEmpty(currentUsername))
        {
            showError("Please Enter a Username!");
            return;
        }
        
        // set this as the current user in SaveManager
        SaveManager.setCurrentUser(currentUsername);
        PlayerPrefs.SetString("LastUsername", currentUsername);
        PlayerPrefs.Save();
        
        Debug.Log("Current username set to: " + currentUsername);
        
        // check if this user already exists
        if (SaveManager.userExists(currentUsername))
        {
            // returning
            string plushyName = SaveManager.loadString("PlushyName", "Unknown");
            
            if (savedText != null)
            {
                savedText.text = "Welcome back, " + currentUsername + "!\nYour plushy " + plushyName + " missed you!";
                savedText.gameObject.SetActive(true);
            }
            
            Debug.Log("Welcome back, " + currentUsername + "! Plushy: " + plushyName);
        }
        else
        {
            // new user
            SaveManager.createNewUser(currentUsername);
            
            if (savedText != null)
            {
                savedText.text = "Welcome, " + currentUsername + "!";
                savedText.gameObject.SetActive(true);
            }
            
            Debug.Log("New user created: " + currentUsername + " (no plushy yet)");
        }
        
        // disable button
        enterButton.interactable = false;
        
        // load game
        StartCoroutine(loadGameWithDelay(2f));
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
    
    IEnumerator loadGameWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadSceneAsync("MainGame");
    }

    IEnumerator loadIntroGameWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // hide intro panel
        if (introPanel != null)
        {
            introPanel.SetActive(false);
        }
        
        // show username panel
        if (usernamePanel != null)
        {
            usernamePanel.SetActive(true);
        }
    }
}