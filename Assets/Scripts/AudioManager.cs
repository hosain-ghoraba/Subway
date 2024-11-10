using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    private AudioSource audioSource;

    [SerializeField] Toggle muteCheckMark;
    [SerializeField] AudioClip pauseScreenSound;
    [SerializeField] AudioClip gameSound;
    private float gameSoundLastStopTime = 0f;

    // ------------------------------------------------------------------ singleton pattern
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = gameObject.AddComponent<AudioSource>();
            // loop
            audioSource.loop = true;
            audioSource.volume = 0.2f;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ---------------------------------------------------------------------- change background music
    public void PlayPauseScreenSound()
    {
        if (audioSource.clip == gameSound)
            gameSoundLastStopTime = audioSource.time;
        audioSource.clip = pauseScreenSound;
        audioSource.Play();
    }

    public void PlayGameSound(bool fromStart = false)
    {
        if (fromStart)
            gameSoundLastStopTime = 0f;
        audioSource.clip = gameSound;
        audioSource.time = gameSoundLastStopTime;
        audioSource.Play();
    }

    public AudioSource GetAudioSource()
    {
        return audioSource;
    }

}
