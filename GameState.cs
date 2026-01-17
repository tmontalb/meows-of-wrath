using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
    public static GameState I;

    public string lastDoorId;
    public bool respawnAtLastDoor;
    public bool doubleJump = false;

    // -1 means "not initialized yet"
    public int playerHealth = -1;

    // Secrets that have been permanently revealed
    public List<string> revealedSecretIds = new List<string>();

    private void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
        DontDestroyOnLoad(gameObject);

        if (MusicManager.I != null)
            MusicManager.I.PlayMusic();
    }

    // Returns true if a secret with this ID was already revealed.
    public bool IsSecretRevealed(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;
        return revealedSecretIds.Contains(id);
    }

    // Old convenience version: mark a secret as revealed.
    // (Always sets value = true.)
    public void SetSecretRevealed(string id)
    {
        SetSecretRevealed(id, true);
    }

    // New version: set or unset a secret.
    // value = true  --> ensure it's in the list
    // value = false --> remove it from the list
    public void SetSecretRevealed(string id, bool value)
    {
        if (string.IsNullOrEmpty(id)) return;

        if (value)
        {
            if (!revealedSecretIds.Contains(id))
                revealedSecretIds.Add(id);
        }
        else
        {
            revealedSecretIds.Remove(id);
        }
    }

    public void ResetForNewRun()
    {
        lastDoorId = null;
        respawnAtLastDoor = false;
        doubleJump = false;
        playerHealth = -1;
        revealedSecretIds.Clear();
    }
}
