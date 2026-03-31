using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SanityManagerScript : MonoBehaviour
{
    public int maxSanity = 100;
    public int currentSanity;
    public SanityBarScript sanityBar;
    
    // sanity decay settings
    public float sanityDecayRate = 1f; // points lost per second
    private float decayTimer = 0f;
    
    // state tracking
    public bool isLowSanity = false;
    public bool isCriticalSanity = false;

    // camera shake
    private ShakeCameraScript cameraShake;

    // scripts
    private PlushyScript plushyScript;
     private ComfortModeScript comfortMode;

    // auto save
    private float saveTimer = 0f;
    private float saveInterval = 30f;
    
    void Start()
    {
        //currentSanity = maxSanity;
        //sanityBar.setMaxSanity(maxSanity);

        cameraShake = Camera.main.GetComponent<ShakeCameraScript>();
        plushyScript = FindObjectOfType<PlushyScript>();
        comfortMode = FindObjectOfType<ComfortModeScript>();

        // load saved sanity for current user
        currentSanity = SaveManager.loadInt("CurrentSanity", maxSanity);
        Debug.Log("Loaded sanity for " + SaveManager.getCurrentUser() + ": " + currentSanity);
        
        sanityBar.setMaxSanity(maxSanity);
        sanityBar.setSanity(currentSanity);
    }
    
    void Update()
    {
        var keyboard = Keyboard.current;

        // pause sanity system while comfort mode is active or ig game not started
        if (comfortMode != null && comfortMode.isActive)
        {
            return;
        }
        if (plushyScript != null && !plushyScript.gameStarted)
        {
            return;
        }

        // decay sanity over time
        decayTimer += Time.deltaTime;
        if (decayTimer >= 1f / sanityDecayRate)
       {
            loseSanity(1);
            decayTimer = 0f;
        }
        
        // check thresholds
        checkSanityThresholds();
        
        // testing: press space to lose sanity quickly
        if (keyboard.qKey.wasPressedThisFrame)
        {
            loseSanity(10);
        }

        // continuous camera shake
        if (currentSanity <= 25)
        {
            // intense continuous shake at critical
            cameraShake.continuousShake = true;
            cameraShake.continuousMagnitude = 0.08f;
        }
        else if (currentSanity <= 50)
        {
            // mild continuous shake at low
            cameraShake.continuousShake = true;
            cameraShake.continuousMagnitude = 0.02f;
        }
        else
        {
            // no shake at normal sanity
            cameraShake.continuousShake = false;
        }

        // auto save every 30 seconds
        saveTimer += Time.deltaTime;
        if (saveTimer >= saveInterval)
        {
            saveGame();
            saveTimer = 0f;
        }
    }

    // save game
    public void saveGame()
    {
        SaveManager.saveInt("CurrentSanity", currentSanity);
    }
    
    // lose sanity (like taking damage)
    public void loseSanity(int amount)
    {
        currentSanity -= amount;
        if (currentSanity < 0)
            currentSanity = 0;
        
        sanityBar.setSanity(currentSanity);
        saveGame(); // save when losing sanity
    }
    
    // gain sanity (when player feeds, plays, cleans)
    public void gainSanity(int amount)
    {
        currentSanity += amount;
        if (currentSanity > maxSanity)
            currentSanity = maxSanity;
        
        sanityBar.setSanity(currentSanity);
    }
    
    // check sanity thresholds and trigger events
    void checkSanityThresholds()
    {
        // check for low sanity (50%)
        if (currentSanity <= 50 && !isLowSanity)
        {
            isLowSanity = true;
            onLowSanity();
        }
        else if (currentSanity > 50 && isLowSanity)
        {
            isLowSanity = false;
        }
        
        // check for critical sanity (25%)
        if (currentSanity <= 25 && !isCriticalSanity)
        {
            isCriticalSanity = true;
            onCriticalSanity();
        }
        else if (currentSanity > 25 && isCriticalSanity)
        {
            isCriticalSanity = false;
        }
        
        // check for game over (0%)
        if (currentSanity <= 0)
        {
            if(!comfortMode.isActive)
            {
                onGameOver();
            }
            return; // doesn't process below 0
        }
    }
    
    // called when sanity drops below 50%
    void onLowSanity()
    {
        Debug.Log("Low sanity warning!");
        // screen shake
        cameraShake.duration = 0.5f;
        cameraShake.magnitude = 0.2f;
    }
    
    // called when sanity drops below 25%
    void onCriticalSanity()
    {
        Debug.Log("Critical sanity!");

        // show angry expression
        plushyScript.showExpression("angry");

        // intense screen shake
        cameraShake.duration = 0.8f;
        cameraShake.magnitude = 0.3f;
    }
    
    // called when sanity reaches 0%
    void onGameOver()
    {
        //Debug.Log("Sanity reached 0, starting Comfort Mode");
        currentSanity = 0; // prevent going negative
        saveGame(); 

        // start comfort mode
        if (comfortMode != null)
        {
            comfortMode.startComfortMode();
        }
    }
}