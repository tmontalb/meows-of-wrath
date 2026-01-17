using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Door : MonoBehaviour
{
    [SerializeField] Transform posToGo;
 //   [SerializeField] GameObject keyTxt; //this is to highlight a text on screen when we are colliding with the gameobject
        /* How tos
        Explanation on how to create a gameobject that will handle its own collisions without requiring raycast:
        Need to create a gameObject with boxcollider2d
        tick "isTrigger"
        if you need for it to respect gravity, add a rigibody2d with body type: kinematic
        put the gameobject in a layer that whatever it will interact with will be included as a layermask
        Deal with the collision scripting in the gameobject's own script

        Explanation on how to adjust when switching to a static variable. player.doubleJump then becomes Player.doublejump and whatever.script(gameobject.collider).doubleJump becomes Player.doubleJump.
        */

    public TextMeshProUGUI cueText;
    public string originalText = "";
    public bool playerDetected;
    GameObject player;
    GameObject cameraFollow;
    public int newLevel = 0;
    public string destination;

    void Start()
    {
        playerDetected = false;
        originalText = cueText.text;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerDetected)
        {
            cueText.text = "Up arrow: Go through door";
            if (Input.GetKeyDown(KeyCode.UpArrow) && Time.timeScale !=0)
            {
                if (newLevel == 0)
                {
                    goThroughDoor(posToGo);
                    GameState.I.lastDoorId = posToGo.name;
                }
                else
                {
                    Player.currentHealth = Player.health;
                    SceneManager.LoadScene(this.destination);
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerDetected = true;
            player = collision.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerDetected = false;
            cueText.text = originalText;
        }
    }

    public void goThroughDoor(Transform targetPosition){
        player.transform.position = new Vector3(targetPosition.position.x, targetPosition.position.y, player.transform.position.z) ;
        cameraFollow = GameObject.FindGameObjectWithTag("MainCamera");
        cameraFollow.GetComponent<CameraFollow>().enabled = false;
        cameraFollow.GetComponent<CameraFollow>().enabled = true;
        playerDetected = false;
    }
}
