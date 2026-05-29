using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource sfxSource;
    public AudioSource ambientSource;

    [Header("Sound Effects")]
    public AudioClip doorSound;
    public AudioClip zoomInSound;
    public AudioClip hitSound;
    public AudioClip missSound;
    //public AudioClip dialogueSound;
    public AudioClip gameOverSound;

    [Header("Ambient Sound")]
    public AudioClip ambientSound;

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    

    // Plays looping ambient audio for the whole game
    public void PlayAmbient()
    {
        if (ambientSource == null)
        {
            Debug.LogError("Ambient Source is missing!");
            return;
        }

        if (ambientSound == null)
        {
            Debug.LogError("Ambient Sound clip is missing!");
            return;
        }

        ambientSource.clip = ambientSound;
        ambientSource.loop = true;
        ambientSource.volume = 0.4f;
        ambientSource.spatialBlend = 0f; // Makes it 2D
        ambientSource.Play();

        Debug.Log("Ambient sound started");
    }

    public void StopAmbient()
    {
        if (ambientSource != null && ambientSource.isPlaying)
        {
            ambientSource.Stop();
        }
    }
    public void PlayGameOverSound()
    {
        PlaySFX(gameOverSound);
    }

    public void PlayDoorSound()
    {
        PlaySFX(doorSound);
    }

    public void PlayZoomInSound()
    {
        PlaySFX(zoomInSound);
    }

    public void PlayHitSound()
    {
        PlaySFX(hitSound);
    }

    public void PlayMissSound()
    {
        PlaySFX(missSound);
    }

   

    // Generic SFX player
    private void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}
