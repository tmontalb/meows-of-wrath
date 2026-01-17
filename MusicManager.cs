using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public static MusicManager I;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float defaultVolume; // will be set from AudioSource.volume

    private Coroutine currentTransition;

    public AudioClip CurrentClip => audioSource != null ? audioSource.clip : null;

    void Awake()
    {
        if (I == null)
        {
            I = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource != null)
        {
            defaultVolume = audioSource.volume;
        }
        else
        {
            Debug.LogWarning("[MusicManager] No AudioSource found on MusicManager GameObject.");
        }
    }
    
    // Old API
    public void PlayMusic()
    {
        if (audioSource == null) return;
        if (!audioSource.isPlaying)
            audioSource.Play();
    }

    public void StopMusic()
    {
        if (audioSource == null) return;
        audioSource.Stop();
    }

    public void PauseMusic()
    {
        if (audioSource == null) return;
        audioSource.Pause();
    }

    public void ResumeMusic()
    {
        if (audioSource == null) return;
        audioSource.UnPause();
    }

    // === New helpers ============================================

    // Immediately switch to this clip and play it (no fade).
    public void PlayImmediate(AudioClip clip, bool loop = true)
    {
        if (audioSource == null || clip == null) return;

        if (currentTransition != null)
        {
            StopCoroutine(currentTransition);
            currentTransition = null;
        }

        audioSource.clip = clip;
        audioSource.loop = loop;

        // We keep whatever the AudioSource currently has (the Inspector value or last fade target).

        audioSource.Play();
    }

    // Fade out current music, swap to new clip, fade back in.
    public void TransitionTo(AudioClip newClip, float fadeDuration = 1f, bool loop = true)
    {
        if (audioSource == null || newClip == null) return;

        // If already playing that clip, no need to transition again
        if (audioSource.clip == newClip && audioSource.isPlaying)
            return;

        if (currentTransition != null)
        {
            StopCoroutine(currentTransition);
        }

        currentTransition = StartCoroutine(TransitionCoroutine(newClip, fadeDuration, loop));
    }

    private IEnumerator TransitionCoroutine(AudioClip newClip, float fadeDuration, bool loop)
    {
        float startVolume = audioSource.volume;
        float t = 0f;

        // Fade out
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float k = t / fadeDuration;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, k);
            yield return null;
        }

        // Swap clip
        audioSource.clip = newClip;
        audioSource.loop = loop;
        audioSource.Play();

        // Fade in
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float k = t / fadeDuration;

            // Here it's fine to use defaultVolume, because we set it from
            // whatever you put in the AudioSource in the Inspector.
            audioSource.volume = Mathf.Lerp(0f, defaultVolume, k);
            yield return null;
        }

        audioSource.volume = defaultVolume;
        currentTransition = null;
    }
}
