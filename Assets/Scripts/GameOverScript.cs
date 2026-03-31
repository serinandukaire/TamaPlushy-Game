using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameOverScript : MonoBehaviour
{
    public static GameOverScript instance;
    
    public GameObject gameOverPanel;
    public TextMeshProUGUI statsText;

    // references
    private SanityManagerScript sanityManager;
    private PlushyScript plushyScript;
    
    // naming panel
    public GameObject namingPanel;
    public TMP_InputField newPlushyNameInput;
    public Button confirmNameButton;
    public Button quitButton;
    public Button newPlushyButton;
    public TextMeshProUGUI namingErrorText;
    
    void Awake()
    {
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
        sanityManager = FindObjectOfType<SanityManagerScript>();
        plushyScript = FindObjectOfType<PlushyScript>();

        // hide both panels at start
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        if (namingPanel != null)
        {
            namingPanel.SetActive(false);
        }
        
        // setup button listener
        if (confirmNameButton != null)
        {
            confirmNameButton.onClick.AddListener(onConfirmNewName);
        }
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(onQuitButtonClicked);
        }
        if (newPlushyButton != null)
        {
            newPlushyButton.onClick.AddListener(onNewPlushyButton);
        }
    }

    void onQuitButtonClicked()
    {
        // return to normal music 
        if (AudioManager.instance != null)
        {
            AudioManager.instance.playBackgroundMusic();
        }
        
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

    public void showGameOver()
    {
        Debug.Log("GAME OVER");

        // play game over sound
        if (AudioManager.instance != null)
        {
            AudioManager.instance.playGameOverSound();
        }
        
        // load stats
        int careCycles = SaveManager.loadInt("CareCycles", 0);
        string plushyName = SaveManager.loadString("PlushyName", "Unknown");
        
        // update text
        if (statsText != null)
        {
            statsText.text = plushyName + " stats:\n\n" +
                           "Care Cycles: " + careCycles;
        }
        
        // stop game
        PlushyScript plushy = FindObjectOfType<PlushyScript>();
        if (plushy != null)
        {
            plushy.gameStarted = false;
            plushy.aiEnabled = false;
        }
        
        // show panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    public void onNewPlushyButton()
    {
        // return to normal music 
        if (AudioManager.instance != null)
        {
            AudioManager.instance.playBackgroundMusic();
        }

        // button click
        if (AudioManager.instance != null)
        {
            AudioManager.instance.playButtonClick();
        }

        // hide game over panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        // show naming panel
        if (namingPanel != null)
        {
            namingPanel.SetActive(true);
        }
        
        // clear input field
        if (newPlushyNameInput != null)
        {
            newPlushyNameInput.text = "";
        }
        
        // hide error
        if (namingErrorText != null)
        {
            namingErrorText.gameObject.SetActive(false);
        }
    }

    void onConfirmNewName()
    {
        // button click
        if (AudioManager.instance != null)
        {
            AudioManager.instance.playButtonClick();
        }

        string newPlushyName = newPlushyNameInput.text.Trim();
        
        // validate
        if (string.IsNullOrEmpty(newPlushyName))
        {
            showNamingError("Please name your new plushy!");
            return;
        }
        
        // get current username
        string username = SaveManager.getCurrentUser();
        
        // reset data for user
        SaveManager.saveString("PlushyName", newPlushyName);
        SaveManager.saveInt("CurrentSanity", 100);
        SaveManager.saveInt("CareCycles", 0);
        
        Debug.Log("New plushy created: " + newPlushyName);
        
        // disable button
        if (confirmNameButton != null)
        {
            confirmNameButton.interactable = false;
        }
        
        // reload game scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    void showNamingError(string message)
    {
        if (namingErrorText != null)
        {
            namingErrorText.text = message;
            namingErrorText.gameObject.SetActive(true);
        }
    }
}