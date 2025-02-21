using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    
    #region Singleton Pattern
    public static MusicPlayer Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }
    #endregion

    public AudioSource audioSource;
    // List of all the music clips
    public AudioClip[] audioClips;

    // Current index being played
    private int currentClipIndex = 0;

    // Play the music clip
    public void Play()
    {
        // Do not play if the audio source is already playing
        if (audioSource.isPlaying)
        {
            return;
        }
        // Do not reset the current clip if the clip is the same
        if (audioSource.clip == audioClips[currentClipIndex])
        {
            audioSource.Play();
            return;
        }
        audioSource.clip = audioClips[currentClipIndex];
        audioSource.Play();
    }

    // Pause the music clip
    public void Pause()
    {
        audioSource.Pause();
    }

    // Skip to the next music clip
    public void Skip()
    {
        currentClipIndex++;
        if (currentClipIndex >= audioClips.Length)
        {
            currentClipIndex = 0;
        }
        // Check if the audio source is playing and stop it
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        audioSource.clip = audioClips[currentClipIndex];
        audioSource.Play();
    }
}
