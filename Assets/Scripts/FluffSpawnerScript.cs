using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluffSpawnerScript : MonoBehaviour
{
    // reference to fluff prefab
    public GameObject fluffPrefab;

    // reference to plushy
    public GameObject plushy;

    // spawn settings
    public float spawnInterval = 7f; // spawn every 7 seconds
    private float spawnTimer = 0f;

    // spawn area bounds
    public float minX = -6f;
    public float maxX = 6f;
    public float minY = -4;
    public float maxY = -1.3f;

    // maximum fluff on screen at once
    public int maxFluffCount = 5;

    // minimum distance from plushy and objects
    public float minDistanceFromPlushy = 2f;
    public float minDistanceFromObjects = 1.5f;

    // multi-spawn settings
    public float chanceForMultipleFluff = 0.3f; // 30 precent chance mult fluff

    // for game started
    private PlushyScript plushyScript;

    // sanity
    private SanityManagerScript sanityScript;

    // comfort
    private ComfortModeScript comfortMode;
    
    void Start()
    {
        plushyScript = FindObjectOfType<PlushyScript>();
        sanityScript = FindObjectOfType<SanityManagerScript>();
        comfortMode = FindObjectOfType<ComfortModeScript>();
    }
    
    void Update()
    {
        if (sanityScript.currentSanity < 25)
        {
            spawnInterval = 1f; // spawn faster
        }
        else if (sanityScript.currentSanity < 50){
            spawnInterval = 3f; // spawn faster
        } 
        else
        {
            spawnInterval = 7f;
        }

        if (plushyScript.gameStarted == true && !comfortMode.isActive)
        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnInterval)
            {
                spawnFluffBurst();
                spawnTimer = 0f;
            }
        }
    }
    
    void spawnFluffBurst()
    {
        // decide how many fluff to spawn
        int fluffToSpawn = 1; // default: spawn 1
        
        // random chance for fluff
        float randomChance = Random.Range(0f, 1f);
        if (randomChance < chanceForMultipleFluff)
        {
            fluffToSpawn = Random.Range(1, 2); // spawn 1 or 2
            Debug.Log("Multi-spawn! Spawning " + fluffToSpawn + " fluff");
        }
        
        // spawn the fluff
        for (int i = 0; i < fluffToSpawn; i++)
        {
            spawnFluff();
        }
    }
    
    void spawnFluff()
    {
        // check if we've reached max fluff
        int currentFluffCount = FindObjectsOfType<ClickFluffScript>().Length;
        if (currentFluffCount >= maxFluffCount)
        {
            Debug.Log("Max fluff reached, not spawning more");
            return;
        }
        
        // try to find a valid spawn position
        Vector3 spawnPosition = Vector3.zero;
        bool validPositionFound = false;
        int attempts = 0;
        int maxAttempts = 100;
        
        while (!validPositionFound && attempts < maxAttempts)
        {
            // random position within bounds
            float randomX = Random.Range(minX, maxX);
            float randomY = Random.Range(minY, maxY);
            spawnPosition = new Vector3(randomX, randomY, 0f);
            bool canSpawnHere = true;
            
            // check if overlapping any UI blocker zones
            Collider2D hit = Physics2D.OverlapPoint(spawnPosition);
            if (hit != null && hit.CompareTag("FluffBlocker"))
            {
                canSpawnHere = false;
            }
            
            // check distance from plushy
            if (plushy != null && canSpawnHere)
            {
                float distanceFromPlushy = Vector3.Distance(spawnPosition, plushy.transform.position);
                if (distanceFromPlushy < minDistanceFromPlushy)
                {
                    canSpawnHere = false;
                }
            }
            
            // check distance from food and toy icons
            if (canSpawnHere)
            {
                GameObject[] interactables = GameObject.FindGameObjectsWithTag("Interactables");
                foreach (GameObject obj in interactables)
                {
                    float distance = Vector3.Distance(spawnPosition, obj.transform.position);
                    if (distance < minDistanceFromObjects)
                    {
                        canSpawnHere = false;
                        break;
                    }
                }
            }
            
            // check distance from other existing fluff
            if (canSpawnHere)
            {
                ClickFluffScript[] existingFluff = FindObjectsOfType<ClickFluffScript>();
                foreach (ClickFluffScript fluff in existingFluff)
                {
                    float distance = Vector3.Distance(spawnPosition, fluff.transform.position);
                    if (distance < 1f) // fluff shouldn't spawn too close to each other
                    {
                        canSpawnHere = false;
                        break;
                    }
                }
            }
            
            if (canSpawnHere)
            {
                validPositionFound = true;
            }
            
            attempts++;
        }
        
        if (validPositionFound)
        {
            // spawn the fluff
            Instantiate(fluffPrefab, spawnPosition, Quaternion.identity);
            //Debug.Log("Fluff spawned at " + spawnPosition);
        }
        else
        {
            Debug.Log("Could not find valid spawn position for fluff after " + maxAttempts + " attempts");
        }
    }
}