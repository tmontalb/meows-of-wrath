using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : MonoBehaviour
{
    int playerDetected;
    GameObject playerGameObject;
    int ennemyDetected;
    GameObject ennemyGameObject;

    // Start is called before the first frame update
    void Start()
    {
        playerDetected = -1;
        ennemyDetected = -1;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerDetected == 1 && Time.timeScale != 0)
        {
            if (playerGameObject.GetComponent<Player>() != null)
            {
                {
                    if (Input.GetAxisRaw("Vertical") == 1 || (Input.GetAxisRaw("Vertical") == -1 && !playerGameObject.gameObject.GetComponent<Player>().collisionNPC))
                    {
                        playerGameObject.GetComponent<Player>().onLadder = true;
                        playerGameObject.transform.position = new Vector2(GetComponent<Renderer>().bounds.center.x, playerGameObject.transform.position.y);
                    }
                }
            }
        }
        else if (ennemyDetected == 1 && Time.timeScale != 0)
        {
            if (ennemyGameObject.GetComponent<Ennemy>() != null)
            {
                if (ennemyGameObject.GetComponent<Ennemy>().directionalInput.y != 0)
                {
                    ennemyGameObject.GetComponent<Ennemy>().onLadder = true;
                    ennemyGameObject.transform.position = new Vector2(GetComponent<Renderer>().bounds.center.x, ennemyGameObject.transform.position.y);
                }
            }
        }
        else
        {
            if (playerDetected == 0)
            {
                if (playerGameObject.GetComponent<Player>() != null)
                {
                    playerGameObject.GetComponent<Player>().onLadder = false;
                    playerDetected = -1;
                }
            }
            else if (ennemyDetected == 0)
            {
                if (ennemyGameObject.GetComponent<Ennemy>() != null)
                {
                    ennemyGameObject.GetComponent<Ennemy>().onLadder = false;
                    ennemyDetected = -1;
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerDetected = 1;
            playerGameObject = collision.gameObject;
        }
        if (collision.CompareTag("Ennemy"))
        {
            ennemyDetected = 1;
            ennemyGameObject = collision.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerDetected = 0;
        }
        if (collision.CompareTag("Ennemy"))
        {
            ennemyDetected = 0;
        }
    }
}
