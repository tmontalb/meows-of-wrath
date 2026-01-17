using UnityEngine;

public class SecretWall : MonoBehaviour
{
    [Header("Secret ID (unique name for this secret)")]
    [SerializeField] private string secretId;

    [Header("What to reveal")]
    [SerializeField] private GameObject[] revealOnBreak; // secret room content root(s)

    [Header("What to hide/disable")]
    [SerializeField] private Collider2D wallCollider;    // collider that blocks the player
    [SerializeField] private Renderer wallRenderer;      // MeshRenderer or SpriteRenderer, etc.

    [Header("Options")]
    [SerializeField] private bool disableGameObject = false;

    private bool revealed;

    private void Reset()
    {
        wallCollider = GetComponent<Collider2D>();
        wallRenderer = GetComponent<Renderer>();
    }

    private void Awake()
    {
        // Default: hide secret content
        if (revealOnBreak != null)
        {
            foreach (var go in revealOnBreak)
                if (go != null) go.SetActive(false);
        }

        // If no explicit ID set, fall back to GameObject name
        if (string.IsNullOrEmpty(secretId))
        {
            secretId = gameObject.name;
        }

        // If GameState says this secret was already revealed, start opened
        if (GameState.I != null && GameState.I.IsSecretRevealed(secretId))
        {
            InternalRevealFromSave();
        }
    }

    // Called by Controller2D when the player is pressing into the wall.
    public void Reveal()
    {
        if (revealed) return;
        revealed = true;

        // Persist this secret as revealed for future loads
        if (GameState.I != null)
        {
            GameState.I.SetSecretRevealed(secretId, true);
        }

        InternalRevealVisuals();
    }

    // Used when loading from GameState so we don't re-write state.
    private void InternalRevealFromSave()
    {
        revealed = true;
        InternalRevealVisuals();
    }

    // Actually show the secret and remove the wall visuals/collider.
    private void InternalRevealVisuals()
    {
        // Reveal secret content
        if (revealOnBreak != null)
        {
            foreach (var go in revealOnBreak)
                if (go != null) go.SetActive(true);
        }

        // Remove/hide wall
        if (wallCollider != null) wallCollider.enabled = false;
        if (wallRenderer != null) wallRenderer.enabled = false;

        if (disableGameObject)
            gameObject.SetActive(false);
    }
}
