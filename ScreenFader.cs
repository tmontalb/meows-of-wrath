using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader I;

    [Header("Setup")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private Color fadeColor = Color.black;

    private void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
        DontDestroyOnLoad(gameObject);

        if (fadeImage == null)
            fadeImage = GetComponentInChildren<Image>(true);

        if (fadeImage == null)
        {
            Debug.LogError("[ScreenFader] No Image assigned or found in children.");
            return;
        }

        // Force full-screen overlay and make sure it actually renders on top
        var rt = fadeImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // Make sure the Canvas is on top of everything
        var canvas = fadeImage.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;   // above pause menus etc.
        }

        fadeImage.enabled = true;
        fadeImage.raycastTarget = false;

        // Start fully transparent
        Color c = fadeColor;
        c.a = 0f;
        fadeImage.color = c;
    }

    // Convenience: reload a scene with fade
    public void ReloadSceneWithFade(string sceneName, float fadeOut = 0.3f, float fadeIn = 0.3f)
    {
        StartCoroutine(FadeOutIn(() =>
        {
            SceneManager.LoadScene(sceneName);
        }, fadeOut, fadeIn));
    }

    public IEnumerator FadeOutIn(Action middleAction, float fadeOutTime, float fadeInTime)
    {
        if (fadeImage == null)
        {
            middleAction?.Invoke();
            yield break;
        }

        fadeImage.enabled = true;
        fadeImage.raycastTarget = true;

        // --- Fade OUT ---
        float t = 0f;
        Color c = fadeColor;

        while (t < fadeOutTime)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / fadeOutTime);
            c.a = k;
            fadeImage.color = c;
            yield return null;
        }

        c.a = 1f;
        fadeImage.color = c;

        middleAction?.Invoke();
        yield return null; // let scene load

        // Refresh reference in case of scene load shenanigans
        if (fadeImage == null)
        {
            fadeImage = GetComponentInChildren<Image>(true);
            if (fadeImage == null)
            {
                yield break;
            }
        }

        // Snap to fully opaque after load
        c = fadeColor;
        c.a = 1f;
        fadeImage.color = c;

        // --- Fade IN ---
        t = 0f;
        while (t < fadeInTime)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / fadeInTime);
            c.a = 1f - k;
            fadeImage.color = c;
            yield return null;
        }

        c.a = 0f;
        fadeImage.color = c;
        fadeImage.raycastTarget = false;
    }

    // Debug helper: press F to test fade without dying
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            StartCoroutine(FadeOutIn(() =>
            {
            }, 0.3f, 0.3f));
        }
    }
}
