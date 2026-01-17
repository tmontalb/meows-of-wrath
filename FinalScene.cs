using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class FinalScene : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text[] options;      // 0 = Main Menu, 1 = Quit
    [SerializeField] private RectTransform selector;  // drag Selector's RectTransform
    [SerializeField] private Vector2 selectorOffset = new Vector2(0f, -40f); // under text

    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "Mainmenu";

    int index = 0;

    void Start()
    {
        if (MusicManager.I != null)
        {
            // Option B: if you prefer to fully stop instead:
            MusicManager.I.StopMusic();
        }
        Time.timeScale = 1f;
        UpdateVisuals();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            index = (index - 1 + options.Length) % options.Length;
            UpdateVisuals();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            index = (index + 1) % options.Length;
            UpdateVisuals();
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            Activate();
        }
    }

    void UpdateVisuals()
    {
        // highlight selected
        for (int i = 0; i < options.Length; i++)
        {
            options[i].alpha = (i == index) ? 1f : 0.6f;
            options[i].fontStyle = (i == index) ? FontStyles.Bold : FontStyles.Normal;
        }

        // move selector in UI space
        if (selector != null && options[index] != null)
        {
            RectTransform target = options[index].rectTransform;

            // Put selector under same parent as target so anchoredPosition matches
            if (selector.parent != target.parent)
                selector.SetParent(target.parent, worldPositionStays: false);

            selector.anchoredPosition = target.anchoredPosition + selectorOffset;
        }
        else
        {
            if (selector == null) Debug.LogError("Selector is not assigned in Inspector.");
        }
    }

    void Activate()
    {
        if (index == 0)
        {
            string url = "https://docs.google.com/spreadsheets/d/1XQPnTE0Kz9YPMMGjbL6x0pqXLszUxoB87j4dDcrXl_I/edit?usp=sharing";
            Application.OpenURL(url);
        }
        else if (index == 1)
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else if (index == 2)
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
