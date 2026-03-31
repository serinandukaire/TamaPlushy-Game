using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    
    [Header("Audio Sources")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource sfxSource;
    
    [Header("Music")]
    public AudioClip backgroundMusic;
    public AudioClip stressedMusic;
    
    [Header("Sound Effects")]
    public AudioClip buttonClick;
    public AudioClip cleaningSound;
    public AudioClip happySound;
    public AudioClip angrySound;
    public AudioClip thudSound;
    public AudioClip gameOverSound;
    
    void Awake()
    {
        // singleton
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // persist across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // play background music on loop
        playBackgroundMusic();
    }
    
    public void playBackgroundMusic()
    {
        if (musicSource != null && backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.volume = 0.3f; // adjust volume
            musicSource.Play();
        }
    }
    
    public void playStressedMusic()
    {
        if (musicSource != null && stressedMusic != null)
        {
            musicSource.Stop();
            musicSource.clip = stressedMusic;
            musicSource.loop = true;
            musicSource.volume = 0.5f;
            musicSource.Play();
        }
    }
    
    public void playButtonClick()
    {
        playSFX(buttonClick);
    }
    
    public void playCleaningSound()
    {
        playSFX(cleaningSound);
    }
    
    public void playHappySound()
    {
        playSFX(happySound);
    }
    
    public void playAngrySound()
    {
        playSFX(angrySound);
    }

    public void playGameOverSound()
    {
        playSFX(gameOverSound);
    }
    
    public void playThudSound()
    {
        playSFX(thudSound);
    }

    void playSFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
    
    public void setMusicVolume(float volume)
    {
        if (musicSource != null)
        {
            musicSource.volume = volume;
        }
    }
    
    public void setSFXVolume(float volume)
    {
        if (sfxSource != null)
        {
            sfxSource.volume = volume;
        }
    }
}