using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    public GameObject pausePanel;
    public TMP_Text[] options;            // 0=Resume, 1=Restart, 2=Quit
    public RectTransform selector;        // drag SelectorArrow's RectTransform here

    [Header("Input")]
    public KeyCode upKey = KeyCode.UpArrow;
    public KeyCode downKey = KeyCode.DownArrow;
    public KeyCode selectKey = KeyCode.Return;
    public KeyCode pauseKey = KeyCode.Escape;

    [Header("Selector Offset")]
    public Vector2 selectorOffset = new Vector2(-40f, 0f); // left of option text

    private int selectedIndex = 0;
    private bool isPaused = false;

    // Expose paused state so NPC can check it
    public bool IsPaused => isPaused;

    void Start()
    {
        isPaused = false;

        if (pausePanel != null)
            pausePanel.SetActive(false);   // make sure it's hidden on scene load

        UpdateSelectorPosition();
    }

    void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            SetPaused(!isPaused);
        }

        if (!isPaused) return;

        if (Input.GetKeyDown(upKey))
        {
            selectedIndex = (selectedIndex - 1 + options.Length) % options.Length;
            UpdateSelectorPosition();
        }
        else if (Input.GetKeyDown(downKey))
        {
            selectedIndex = (selectedIndex + 1) % options.Length;
            UpdateSelectorPosition();
        }
        else if (Input.GetKeyDown(selectKey))
        {
            ActivateSelection();
        }
    }

    void SetPaused(bool paused)
    {
        isPaused = paused;

        if (pausePanel != null)
            pausePanel.SetActive(paused);

        // Check if a dialog is currently open
        Player player = FindObjectOfType<Player>();
        bool dialogOpen = (player != null && player.dialog);

        if (paused)
        {
            // Only freeze time & pause music if NO dialog is open
            if (!dialogOpen)
            {
                Time.timeScale = 0f;
                if (MusicManager.I != null)
                    MusicManager.I.PauseMusic();
            }

            selectedIndex = Mathf.Clamp(selectedIndex, 0, options.Length - 1);
            UpdateSelectorPosition();
        }
        else
        {
            // Only resume time & music if no dialog is open
            if (!dialogOpen)
            {
                Time.timeScale = 1f;
                if (MusicManager.I != null)
                    MusicManager.I.ResumeMusic();
            }
        }
    }

    void UpdateSelectorPosition()
    {
        if (selector == null || options == null || options.Length == 0) return;

        RectTransform target = options[selectedIndex].rectTransform;
        selector.position = target.position + (Vector3)selectorOffset;
    }

    void ActivateSelection()
    {
        switch (selectedIndex)
        {
            case 0: // Resume
                SetPaused(false);
                break;

            case 1: // Restart
                Time.timeScale = 1f;
                GameState.I.doubleJump = false;
                GameState.I.respawnAtLastDoor = false;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                break;

            case 2: // Quit
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
                break;
        }
    }
}
