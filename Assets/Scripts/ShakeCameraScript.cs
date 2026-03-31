using UnityEngine;
using System.Collections;

public class ShakeCameraScript : MonoBehaviour
{
    public bool startShake = false;
    public float duration = 1f; // duration of shake
    public float magnitude = 0.2f;

    // continuos shake
    public bool continuousShake = false;
    public float continuousMagnitude = 0.05f;

    private Vector3 originalPosition;
    
    void Start()
    {
        originalPosition = transform.position;
    }

    void Update(){
        // one time shake
        if (startShake){
            startShake = false;
            StartCoroutine(Shaking());
        }
        
        // continuous shake when enabled
        if (continuousShake)
        {
            float x = Random.Range(-1f, 1f) * continuousMagnitude;
            float y = Random.Range(-1f, 1f) * continuousMagnitude;
            transform.position = new Vector3(originalPosition.x + x, originalPosition.y + y, originalPosition.z);
        }
        else
        {
            // return to original when not shaking
            if (!startShake)
            {
                transform.position = originalPosition;
            }
        }

    }
    
    IEnumerator Shaking(){
        Vector3 startPosition = transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < duration){
            elapsedTime += Time.deltaTime;
             // only shake X and Y, keep Z the same
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            transform.position = new Vector3(startPosition.x + x, startPosition.y + y, startPosition.z);
            yield return null;
        }
        transform.position = startPosition;
    }
}
