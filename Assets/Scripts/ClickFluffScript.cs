using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ClickFluffScript : MonoBehaviour
{
    // how much sanity is lost if not cleaned
    public int sanityPenalty = 10;
    
    // references
    private SanityManagerScript sanityManager;
    private ComfortModeScript comfortMode;
    
    // lifetime before it causes sanity loss
    public float lifetimeBeforePenalty = 10f;
    private float timer = 0f;
    private bool hasCausedPenalty = false;
    
    // collider for detection
    private Collider2D objectCollider;

    // hover state
    private bool isHovering = false;
    private Vector3 originalScale;
    private SpriteRenderer spriteRenderer;
    
    void Start()
    {
        sanityManager = FindObjectOfType<SanityManagerScript>();
        comfortMode = FindObjectOfType<ComfortModeScript>();
        objectCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // store original values
        originalScale = transform.localScale;

        // make fluff pulse to indicate it needs cleaning
        StartCoroutine(pulseEffect());
    }
    
    void Update()
    {
        // get mouse position in world space
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        // check if mouse is over this object
        bool mouseOver = objectCollider.OverlapPoint(mousePosition);

        // handle hover state changes
        if (mouseOver && !isHovering)
        {
            onHoverEnter();
        }
        else if (!mouseOver && isHovering)
        {
            onHoverExit();
        }

        // check for mouse click
        var mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame && mouseOver)
        {
            onClicked();
        }
        
        // penalty not during comfort mode
        if(!comfortMode.isActive)
        {
        // after certain time, cause sanity loss
        timer += Time.deltaTime;
        if (timer >= lifetimeBeforePenalty && !hasCausedPenalty)
        {
            if (sanityManager != null)
            {
                sanityManager.loseSanity(sanityPenalty);
                Debug.Log("Fluff not cleaned! -10 sanity");
                hasCausedPenalty = true;
            }
        }
        }
    }

    void onHoverEnter()
    {
        isHovering = true;
        StopAllCoroutines(); // stop any running animations
        StartCoroutine(hoverEffect());
    }
    
    void onHoverExit()
    {
        isHovering = false;
        StopAllCoroutines();
        StartCoroutine(pulseEffect());
        // return to normal
        transform.localScale = originalScale;
    }
    
    void onClicked()
    {
        Debug.Log("Cleaned fluff!");

        // cleaning sound
        if (AudioManager.instance != null)
        {
            AudioManager.instance.playCleaningSound();
        }

        // notify comfort
        if (comfortMode != null && comfortMode.isActive)
        {
           // Debug.Log("Notifying comfort mode - isActive: " + comfortMode.isActive);
            comfortMode.onFluffCleaned();
        }
    
        StopAllCoroutines();
        Destroy(gameObject);
    }
    
    // hover effect
    IEnumerator hoverEffect()
    {
        float time = 0f;
        while (isHovering)
        {
            time += Time.deltaTime * 3f;
            float scale = 1f + Mathf.Sin(time) * 0.1f;
            transform.localScale = originalScale * scale;
            yield return null;
        }
    }
    // pulse effect 
    IEnumerator pulseEffect()
    {
        //Debug.Log("pulse");
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