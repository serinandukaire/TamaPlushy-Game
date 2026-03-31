using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThoughtBubbleScript : MonoBehaviour
{
    // reference to plushy
    public Transform plushy;
    // offset above plushy's head
    public Vector3 offset = new Vector3(-1.07f, 1.8f, 0f);
    
    // icon system
    [System.Serializable]
    public class ThoughtIcon
    {
        public Sprite iconSprite;
        public string needType; // food or toy
        public float iconScale = 1f; // add scale for each icon
    }
    public ThoughtIcon[] possibleIcons; // assign all icons in inspector
    public GameObject iconObject; // the child object that will display the icon
    public Vector3 iconOffset = new Vector3(0f, 0.3f, 0f); // position relative to bubble
    
    // sprite renderers
    private SpriteRenderer bubbleRenderer;
    private SpriteRenderer iconRenderer;
    
    // timing
    public float displayDuration = 5f;
    public float minTimeBetweenThoughts = 3f;
    public float maxTimeBetweenThoughts = 10f;
    private float thoughtTimer = 0f;
    private float nextThoughtTime = 0f;
    
    // state tracking
    private bool isVisible = false;
    public string currentNeed = "";
    
    // sanity manager
    private SanityManagerScript sanityManager;
    public int sanityPenaltyForIgnoring = 10;
    
    // plushy script
    private PlushyScript plushyScript;
    
    void Start()
    {
        bubbleRenderer = GetComponent<SpriteRenderer>();
        sanityManager = FindObjectOfType<SanityManagerScript>();
        plushyScript = FindObjectOfType<PlushyScript>();
        
        // setup icon object
        if (iconObject != null)
        {
            iconRenderer = iconObject.GetComponent<SpriteRenderer>();
        }
        
        // start hidden
        hideThoughtBubble();
        // set first random time
        nextThoughtTime = Random.Range(minTimeBetweenThoughts, maxTimeBetweenThoughts);
    }
    
    void Update()
    {
        // only work if game has started
        if (plushyScript.gameStarted == true)
        {
            // follow plushy
            if (plushy != null)
            {
                transform.position = plushy.position + offset;
               
                
                // position icon relative to bubble
                if (iconObject != null)
                {
                    iconObject.transform.position = transform.position + iconOffset;
                }
            }
            
            // if bubble is visible, count down display time
            if (isVisible)
            {
                displayDuration -= Time.deltaTime;
                if (displayDuration <= 0f)
                {
                    // time ran out
                    onThoughtIgnored();
                }
            }
            // if bubble is hidden, count up to next thought
            else
            {
                thoughtTimer += Time.deltaTime;
                if (thoughtTimer >= nextThoughtTime)
                {
                    showRandomThought();
                    thoughtTimer = 0f;
                }
            }
        }
    }
    
    void showRandomThought()
    {
        if (possibleIcons.Length == 0)
        {
            Debug.LogWarning("No icons assigned to thought bubble!");
            return;
        }
        
        // randomly choose an icon
        int randomIndex = Random.Range(0, possibleIcons.Length);
        ThoughtIcon chosenIcon = possibleIcons[randomIndex];
        
        // set the icon
        currentNeed = chosenIcon.needType;
        if (iconRenderer != null)
        {
            iconRenderer.sprite = chosenIcon.iconSprite;
             // apply the scale for this specific icon
            iconObject.transform.localScale = Vector3.one * chosenIcon.iconScale;
        }
        
        Debug.Log("Plushy wants: " + currentNeed);
        
        // show bubble and icon
        bubbleRenderer.enabled = true;
        if (iconRenderer != null)
        {
            iconRenderer.enabled = true;
        }
        isVisible = true;
        // reset display timer
        displayDuration = 5f;
    }
    
    void hideThoughtBubble()
    {
        bubbleRenderer.enabled = false;
        if (iconRenderer != null)
        {
            iconRenderer.enabled = false;
        }
        isVisible = false;
        currentNeed = "";
        // set next random time
        nextThoughtTime = Random.Range(minTimeBetweenThoughts, maxTimeBetweenThoughts);
    }
    
    void onThoughtIgnored()
    {
        Debug.Log("Player ignored Plushy's need! -20 sanity");
        
        // show angry expression
        plushyScript.showExpression("angry");
        if (sanityManager != null)
        {
            sanityManager.loseSanity(sanityPenaltyForIgnoring);
        }
        hideThoughtBubble();
    }
    
    // call this when player fulfills the need
    public void onNeedFulfilled(string needType)
    {
        if (isVisible && currentNeed == needType)
        {
            Debug.Log("Player fulfilled Plushy's need! +20 sanity");
            hideThoughtBubble();

            // show loved expression
            plushyScript.showExpression("loved");

            // give sanity reward for fulfilling need
            sanityManager.gainSanity(20);
        }
    }
}