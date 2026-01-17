using UnityEngine;

public class LevelMusicController : MonoBehaviour
{
    [Header("Default music for this scene")]
    public AudioClip defaultLevelMusic;

    [Tooltip("If false, this scene won't auto-start music.")]
    public bool playOnStart = true;

    private void Start()
    {
        if (!playOnStart) return;
        if (MusicManager.I == null) return;
        if (defaultLevelMusic == null) return;

        // Force this scene's default music
        MusicManager.I.PlayImmediate(defaultLevelMusic, loop: true);
    }
}
