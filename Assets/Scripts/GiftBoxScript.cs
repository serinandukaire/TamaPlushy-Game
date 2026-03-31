using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GiftBoxScript : MonoBehaviour
{
    public static GiftBoxScript instance;
    
    // reference to plushy
    public Transform plushy;
    
    // sprite renderer
    private SpriteRenderer boxRenderer;
    
    // state tracking
    public bool isVisible = true;
    
    // plushy script
    private PlushyScript plushyScript;
    private PlushyNameScript nameScript;
    private Vector3 originalScale;

    // offset for box
    private Vector3 offset = new Vector3(0f, -0.3f, 0f);
    
    // collider for click detection
    private Collider2D boxCollider;
    
    void Awake()
    {
        // SINGLETON
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
        boxRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<Collider2D>();
        plushyScript = FindObjectOfType<PlushyScript>();
        nameScript = FindObjectOfType<PlushyNameScript>();
        
        // CHECK if user already has a plushy name
        string existingPlushyName = SaveManager.loadString("PlushyName", "");
        
        if (!string.IsNullOrEmpty(existingPlushyName))
        {
            // RETURNING USER - they already named their plushy
            Debug.Log("User already has plushy: " + existingPlushyName + ". Skipping gift box.");
            
            isVisible = false; // mark as not visible
            Destroy(gameObject); // destroy immediately
            return; // stop here
        }
        
        // NEW USER - show gift box
        Debug.Log("New user - showing gift box");
        originalScale = transform.localScale;
        StartCoroutine(pulseEffect());
    }

    void Update()
    {
        // follow plushy
        if (plushy != null)
        {
            transform.position = plushy.position + offset;
        }
        
        // check for click on THIS gift box specifically
        var mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            // get mouse position
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(mouse.position.ReadValue());
            
            // check if clicked on gift box
            if (boxCollider != null && boxCollider.OverlapPoint(mousePosition))
            {
                onClicked();
            }
        }
    }
    
    public void onClicked()
    {
        Debug.Log("Gift box clicked!");

        nameScript.showPanel();

        isVisible = false;
        StopAllCoroutines();
        
        // play sound if you have one
        if (AudioManager.instance != null)
        {
            AudioManager.instance.playButtonClick();
        }
        
        Destroy(gameObject);
    }
    
    // pulse effect 
    IEnumerator pulseEffect()
    {
        float time = 0f;
        while (true)
        {
            time += Time.deltaTime * 2f;
            float scale = 1f + Mathf.Sin(time) * 0.15f;
            transform.localScale = originalScale * scale;
            yield return null;
        }
    }
}