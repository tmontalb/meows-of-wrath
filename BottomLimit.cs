using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottomLimit : MonoBehaviour
{
    GameObject playerGameObject;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        playerGameObject = collision.gameObject;
        if (collision.CompareTag("Player"))
        {
            playerGameObject.GetComponent<Player>().Pit();
        }
    }
}
