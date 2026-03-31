using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ClickObjectScript : MonoBehaviour
{
    // reference to sanity manager
    private SanityManagerScript sanityManager;

    // reference to thought bubble
    private ThoughtBubbleScript thoughtBubble;

    // visual feedback
    private SpriteRenderer spriteRenderer;

    // collider for detection
    private Collider2D objectCollider;

    // for game started
    private PlushyScript plushyScript;

    // hover state
    private bool isHovering = false;
    private Vector3 originalScale;
    
    void Start()
    {
        sanityManager = FindObjectOfType<SanityManagerScript>();
        plushyScript = FindObjectOfType<PlushyScript>();
        thoughtBubble = FindObjectOfType<ThoughtBubbleScript>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        objectCollider = GetComponent<Collider2D>();

        // store original values
        originalScale = transform.localScale;
    }
    
    void Update()
    {
        if (plushyScript.gameStarted == true){
            // get mouse position in world space
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

            // check if mouse is over this object
            bool mouseOver = objectCollider.OverlapPoint(mousePosition);

            // handle hover state changes
            if (mouseOver && !isHovering)
            {
                // just started hovering
                onHoverEnter();
            }
            else if (!mouseOver && isHovering)
            {
                // stopped hovering
                onHoverExit();
            }

            // check for mouse click
            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame && mouseOver)
            {
                onClicked();
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

        // return to normal
        transform.localScale = originalScale;
    }
    
    void onClicked()
    {
        // button click
        if (AudioManager.instance != null)
        {
            AudioManager.instance.playButtonClick();
        }
        
        // check click
        if (sanityManager != null)
        {
            // checks interaction type
            string interactionType = thoughtBubble.currentNeed;
            thoughtBubble.onNeedFulfilled(interactionType);
            
            // stop hover effect and do click effect
            StopAllCoroutines();
            StartCoroutine(clickEffect());
        }
    }
    
    // hover effect, gentle pulsing
    IEnumerator hoverEffect()
    {
        float time = 0f;
        while (isHovering)
        {
            time += Time.deltaTime * 3f; // speed of pulse

            // gentle scale pulse
            float scale = 1f + Mathf.Sin(time) * 0.1f; // subtle 10% size change
            transform.localScale = originalScale * scale;
            
            yield return null;
        }
    }
    
    // visual effect when clicked
    IEnumerator clickEffect()
    {
        float duration = 0.3f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            // bounce scale
            float scale = 1f + Mathf.Sin(progress * Mathf.PI) * 0.3f;
            transform.localScale = originalScale * scale;
           
            yield return null;
        }
        // reset
        transform.localScale = originalScale;
        isHovering = false;
    }
}