using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlushyScript : MonoBehaviour
{
    SpriteRenderer mySpriteRenderer;
    Animator myAnimator;
    RuntimeAnimatorController idleAnimation;
    RuntimeAnimatorController walkAnimation;
    RuntimeAnimatorController jumpAnimation;
    RuntimeAnimatorController lovedAnimation;
    RuntimeAnimatorController angryAnimation;

    System.Boolean flipped = false;

    // into sequence 
    public bool gameStarted = false;
    private Vector3 landingPosition = new Vector3(0f, -2f, 0f);
    private Rigidbody2D rb;

    // expression state
    private bool showingExpression = false;
    private SanityManagerScript sanityScript;

    // ai wandering
    public bool aiEnabled = true; // can disable for testing
    private bool playerHasControl = false;
    private Vector3 aiTargetPosition;
    private bool hasAITarget = false;
    private float aiMoveSpeed = 1.5f; // slower than player
    private float aiWaitTime = 2f; // wait at destination
    private float aiWaitTimer = 0f;
    private bool aiIsWaiting = false;
    
    // wandering bounds
    private float minX = -7.8f;
    private float maxX = 7.8f;
    private float groundY = -2f; // stay on ground

    void Start()
    {
        mySpriteRenderer = GetComponent<SpriteRenderer>();
        myAnimator = GetComponent<Animator>();
        sanityScript = FindObjectOfType<SanityManagerScript>();

        idleAnimation = Resources.Load("Animations/PlushyIdleController") as RuntimeAnimatorController;
        walkAnimation = Resources.Load("Animations/PlushyWalkController") as RuntimeAnimatorController;
        jumpAnimation = Resources.Load("Animations/PlushyJumpController") as RuntimeAnimatorController;
        lovedAnimation = Resources.Load("Animations/PlushyLovedController") as RuntimeAnimatorController;
        angryAnimation = Resources.Load("Animations/PlushyAngryController") as RuntimeAnimatorController;

        // check returning user
        string existingPlushyName = SaveManager.loadString("PlushyName", "");

        if (string.IsNullOrEmpty(existingPlushyName))
        {
            transform.position = new Vector3(0f, 10f, 0f);
            
            if (rb != null)
            {
                rb.simulated = true;
                rb.gravityScale = 0.5f;
            }
            
            StartCoroutine(WaitForLanding());
        }
        else
        {
            transform.position = landingPosition;
            
            if (rb != null)
            {
                rb.simulated = false; // disable physics immediately
            }
            
            // start game immediately
            gameStarted = true;
            myAnimator.runtimeAnimatorController = idleAnimation;
        }
    }

    IEnumerator WaitForLanding()
    {
        // wait until plushy reaches near the landing position
        while (transform.position.y > landingPosition.y + 0.3f)
        {
            yield return null;
        }
        
        // play thud sound
        if (AudioManager.instance != null)
        {
            AudioManager.instance.playThudSound();
        }

        // snap to exact landing position
        transform.position = landingPosition;
        
        // disable physics to prevent further movement
        if (rb != null)
            rb.simulated = false;
        
        // small delay after landing
        yield return new WaitForSeconds(0.3f);
        
        // wait for gift box to be clicked
        while (GiftBoxScript.instance != null && GiftBoxScript.instance.isVisible)
        {
            yield return null;
        }
            
        Debug.Log("Gift box clicked, waiting for naming...");
            
        // wait for naming to be complete
        while (PlushyNameScript.instance != null && !PlushyNameScript.instance.namingComplete)
        {
            
            yield return null;
        }
        
        yield return new WaitForSeconds(1.5f);
        Debug.Log("Naming complete!");
        
        // game is now ready to play
        gameStarted = true;
        myAnimator.runtimeAnimatorController = idleAnimation;
    }

    // update is called once per frame
    void FixedUpdate()
    {
        // dont allow movement until game has started
        if (!gameStarted) return;

        if (showingExpression)
        {
            return; // don't move during expression
        }

        float runspeed = 0.5f;
        
        // get keyboard input
        var keyboard = Keyboard.current;
        if (keyboard == null) return; // no keyboard connected
        
        // get horizontal movement
        float moveHorizontal = 0f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            moveHorizontal = -1f;
        else if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            moveHorizontal = 1f;
        
        // check for jump
        bool isJumping = keyboard.spaceKey.isPressed;
        
        // player input detection
        if (moveHorizontal != 0 || isJumping)
        {
            playerHasControl = true;
            hasAITarget = false; // cancel AI target
        }
        else
        {
            playerHasControl = false;
        }

        // handle movement
        if (playerHasControl)
        {
            // determine animation state
            if (isJumping)
            {
                // only change animation if not showing expression
                if (!showingExpression)
                {
                    myAnimator.runtimeAnimatorController = jumpAnimation;
                    // play jump sound
                }

                Vector3 currentPos = transform.position;
                currentPos.y += 0.1f;
                transform.position = currentPos;
            }
            if (moveHorizontal < 0)
            {
                // walk left
                if (!isJumping && !showingExpression) myAnimator.runtimeAnimatorController = walkAnimation;
                flipped = true;
                Vector3 currentPos = transform.position;
                currentPos.x -= 0.1f * runspeed;
                transform.position = currentPos;
            }
            else if (moveHorizontal > 0)
            {
                // walk right
                if (!isJumping && !showingExpression) myAnimator.runtimeAnimatorController = walkAnimation;
                flipped = false;
                Vector3 currentPos = transform.position;
                currentPos.x += 0.1f * runspeed;
                transform.position = currentPos;
            }
            else if (!isJumping)
            {
                // idle only change animation if not showing expression and not critical sanity
                if (!showingExpression)
                {
                    setToIdle();

                }
            }
        } else
        {
            // ai controls
            if (aiEnabled)
            {
                handleAIMovement();
            }
            else if (!showingExpression)
            {
                // no AI, no player input = idle
                setToIdle();
            }
        }
        
        // set sprite direction
        mySpriteRenderer.flipX = flipped;

        // constrain plushy within screen bounds
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -7.8f, 7.8f); // adjust values
       // pos.y = Mathf.Clamp(pos.y, -4.5f, 4.5f); // adjust values
        transform.position = pos;
    }

    void handleAIMovement()
    {
        // stop ai if showing expression
        if (showingExpression)
        {
            // show idle during expression
            if (!showingExpression) // this check is redundant, but keeping for safety
            {
                setToIdle();
            }
            return;
        }
        
        // if waiting at destination
        if (aiIsWaiting)
        {
            aiWaitTimer += Time.deltaTime;
            if (aiWaitTimer >= aiWaitTime)
            {
                aiIsWaiting = false;
                aiWaitTimer = 0f;
                hasAITarget = false;
            }
            
            // always show idle while waiting
            setToIdle();
            return;
        }
        
        // if no target, pick one
        if (!hasAITarget)
        {
            pickNewAITarget();
        }
        
        // move toward target
        if (hasAITarget)
        {
            Vector3 currentPos = transform.position;
            float direction = Mathf.Sign(aiTargetPosition.x - currentPos.x);
            float distanceToTarget = Mathf.Abs(aiTargetPosition.x - currentPos.x);
            
            // check if reached target (increased threshold)
            if (distanceToTarget < 0.3f) // INCREASED from 0.2f
            {
                // arrived at target
                hasAITarget = false;
                aiIsWaiting = true;
                aiWaitTimer = 0f;
                
                // force idle animation
                setToIdle();
            }
            else
            {
                // move toward target
                currentPos.x += direction * aiMoveSpeed * Time.deltaTime;
                transform.position = currentPos;
                
                // face correct direction
                flipped = direction < 0;
                
                // FORCE walk animation
                myAnimator.runtimeAnimatorController = walkAnimation;
            }
        }
        else
        {
            // show idle
            setToIdle();
        }
    }

    void pickNewAITarget()
    {
        // pick random x position within bounds
        float randomX = Random.Range(minX, maxX);
        aiTargetPosition = new Vector3(randomX, groundY, 0f);
        hasAITarget = true;
        
        //Debug.Log("Plushy AI picked new target: " + randomX);
    }

    // show expression
    public void showExpression(string expressionType, float duration = 2f)
    {
        StopAllCoroutines();
        StartCoroutine(expressionTimer(expressionType, duration));
    }

    // return to idle if not critical sanity
    void setToIdle()
    {
        if (sanityScript != null && sanityScript.isCriticalSanity)
        {
            myAnimator.runtimeAnimatorController = angryAnimation;
        }
        else
        {
            myAnimator.runtimeAnimatorController = idleAnimation;
        }
    }

    // temporarily show an expression then return to normal
    IEnumerator expressionTimer(string expressionType, float duration)
    {
        showingExpression = true;
        
        // show the expression
        if (expressionType == "loved")
        {
            //Debug.Log("Showing loved expression");
            myAnimator.runtimeAnimatorController = lovedAnimation;

            // play happy sound
            if (AudioManager.instance != null)
            {
                AudioManager.instance.playHappySound();
            }
        }
        else if (expressionType == "angry")
        {
            //Debug.Log("Showing angry expression");
            myAnimator.runtimeAnimatorController = angryAnimation;

            // play angry sound
            if (AudioManager.instance != null)
            {
                AudioManager.instance.playAngrySound();
            }
        }
        
        // wait
        yield return new WaitForSeconds(duration);
        showingExpression = false;
        
        setToIdle();
    }

}