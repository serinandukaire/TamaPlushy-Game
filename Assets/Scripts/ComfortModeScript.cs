using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ComfortModeScript : MonoBehaviour
{
    public GameObject comfortPanel;
    public GameObject sanityBarPanel;
    public TextMeshProUGUI emergencyText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI progressText;

    // fluff spawning
    public GameObject fluffPrefab; // assign in Inspector
    //public int initialFluffCount = 5; // spawn 5 fluff at start
    
    public bool isActive = false;
    private float timeRemaining = 10f;
    private int fluffCleaned = 0;
    [SerializeField] private int fluffRequired = 3;
    
    private SanityManagerScript sanityManager;
    private PlushyScript plushyScript;
    private ShakeCameraScript cameraShake;
    
    void Start()
    {
        sanityManager = FindObjectOfType<SanityManagerScript>();
        plushyScript = FindObjectOfType<PlushyScript>();
        cameraShake = Camera.main.GetComponent<ShakeCameraScript>();
        
        // hide at start
        if (comfortPanel != null)
        {
            comfortPanel.SetActive(false);
        }
    }
    
    void Update()
    {
        if (isActive)
        {
            // countdown timer
            timeRemaining -= Time.deltaTime;
            
            // update UI
            string plushyName = SaveManager.loadString("PlushyName", "Unknown");
            if (emergencyText != null)
            {
                emergencyText.text = plushyName + " needs Love!";
            }
            if (timerText != null)
            {
                timerText.text = "Timer: " + Mathf.CeilToInt(timeRemaining) + "s";
            }
            
            if (progressText != null)
            {
                progressText.text = "Cleaned: " + fluffCleaned + "/" + fluffRequired;
            }
            
            // check win condition
            if (fluffCleaned >= fluffRequired)
            {
                onComfortSuccess();
            }
            // check fail condition
            else if (timeRemaining <= 0f)
            {
                Debug.Log(timeRemaining);
                onComfortFailed();
            }
        }
    }
    
    public void startComfortMode()
    {
        //Debug.Log("Comfort Mode Started!");

        // stressed music
        if (AudioManager.instance != null)
        {
            AudioManager.instance.playStressedMusic();
        }

        // prevent double start
        if(isActive) return;

        isActive = true;
        timeRemaining = 10f;
        fluffCleaned = 0;
        
        // stop plushy movement/ai
        if (plushyScript != null)
        {
            plushyScript.gameStarted = false;
           // plushyScript.aiEnabled = false;
            plushyScript.showExpression("angry", 999f); // stay angry
        }

        // remove any fluff from game
        cleanupAllFluff();

        // spawn fluff for comfort mode
        spawnComfortFluff();
        
        // intense shake
        //if (cameraShake != null)
       // {
           // cameraShake.continuousShake = true;
            //cameraShake.continuousMagnitude = 0.15f;
        //}

        // hide bar panel
        if (sanityBarPanel != null)
        {
            sanityBarPanel.SetActive(false);
        }
        // show panel
        if (comfortPanel != null)
        {
            comfortPanel.SetActive(true);
        }
    }
    
    void spawnComfortFluff()
    {
        int toSpawn = Mathf.Max(0, fluffRequired);

        // spawn fluff in random positions
        for (int i = 0; i < toSpawn; i++)
        {
            float randomX = Random.Range(-6f, 6f);
            float randomY = Random.Range(-4f, -1.3f);
            Vector3 spawnPos = new Vector3(randomX, randomY, 0f);
            
            Instantiate(fluffPrefab, spawnPos, Quaternion.identity);
        }
    }

    public void onFluffCleaned()
    {
        if (isActive)
        {
            fluffCleaned++;
            Debug.Log("Fluff cleaned: " + fluffCleaned + "/" + fluffRequired);
        }
    }
    
    void onComfortSuccess()
    {
        Debug.Log("Comfort Mode Success!");
        
        isActive = false;

        // return to normal music 
        if (AudioManager.instance != null)
        {
            AudioManager.instance.playBackgroundMusic();
        }

        // clean up remaining fluff
        cleanupAllFluff();

        // increment care cycles
        int careCycles = SaveManager.loadInt("CareCycles", 0);
        careCycles++;
        SaveManager.saveInt("CareCycles", careCycles);
            
        Debug.Log(SaveManager.getCurrentUser() + " care cycles: " + careCycles);
        
        // restore sanity
        if (sanityManager != null)
        {
            sanityManager.currentSanity = 50;
            sanityManager.sanityBar.setSanity(50);
            sanityManager.saveGame();
        }
        
        // resume game
        if (plushyScript != null)
        {
            plushyScript.gameStarted = true;
          //  plushyScript.aiEnabled = true; // resume AI
            plushyScript.showExpression("loved", 3f);
        }
        // show bar panel
        if (sanityBarPanel != null)
        {
            sanityBarPanel.SetActive(true);
        }

        // hide panel
        if (comfortPanel != null)
        {
            comfortPanel.SetActive(false);
        }
    }
    
    void onComfortFailed()
    {
        Debug.Log("Comfort Mode Failed - Game Over");
        
        isActive = false;
        enabled = false;
        
        // show game over screen 
        if (GameOverScript.instance != null)
        {
            GameOverScript.instance.showGameOver();
            Debug.Log("GAMESSSSS OVER");
        }
        
    }

    void cleanupAllFluff()
    {
        // destroy all remaining fluff
        ClickFluffScript[] allFluff = FindObjectsOfType<ClickFluffScript>();
        foreach (ClickFluffScript fluff in allFluff)
        {
            Destroy(fluff.gameObject);
        }
        Debug.Log("Cleaned up all fluff");
    }
}