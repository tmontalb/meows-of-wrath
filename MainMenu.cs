using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("Scene")]
    [Tooltip("Name of your gameplay scene (check Build Settings).")]
    [SerializeField] private string gameplaySceneName = "Simple demo";

    [Header("UI")]
    public TMP_Text[] options;     // 0=Start, 1=Quit (optional)
    public RectTransform selector; // optional arrow/marker transform (can be null)

    [Header("Input")]
    public KeyCode upKey = KeyCode.UpArrow;
    public KeyCode downKey = KeyCode.DownArrow;
    public KeyCode selectKey = KeyCode.Return;

    int selectedIndex = 0;

    void Start()
    {
        // Ensure timescale is normal in case you came from a paused state
        Time.timeScale = 1f;

        UpdateVisuals();
        // no MusicManager calls here – menu music is handled by LevelMusicController in this scene
    }

    void Update()
    {
        if (options == null || options.Length == 0) return;

        if (Input.GetKeyDown(upKey))
        {
            selectedIndex = (selectedIndex - 1 + options.Length) % options.Length;
            UpdateVisuals();
        }
        else if (Input.GetKeyDown(downKey))
        {
            selectedIndex = (selectedIndex + 1) % options.Length;
            UpdateVisuals();
        }
        else if (Input.GetKeyDown(selectKey))
        {
            ActivateSelected();
        }
    }

    void ActivateSelected()
    {
        switch (selectedIndex)
        {
            case 0:
                // Start a BRAND NEW RUN: reset persistent state
                if (GameState.I != null)
                {
                    GameState.I.ResetForNewRun();
                }

                // (Optional) pick the correct starting music for the gameplay scene,
                // or just let the MusicManager/MusicZone handle it in the scene.
                // We can even leave music alone here if the level has MusicZones.

                SceneManager.LoadScene(gameplaySceneName);
                break;
            case 1: // Restart
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
                break;
        }
    }

    void UpdateVisuals()
    {
        // Highlight selected text (simple style change)
        for (int i = 0; i < options.Length; i++)
        {
            if (options[i] == null) continue;
            options[i].fontStyle = (i == selectedIndex) ? FontStyles.Bold : FontStyles.Normal;
        }

        // Move selector next to the selected option (if assigned)
        if (selector != null && options[selectedIndex] != null)
        {
            RectTransform target = options[selectedIndex].rectTransform;
            Vector3 pos = selector.position;
            pos.y = target.position.y;
            selector.position = pos;
        }
    }
}
