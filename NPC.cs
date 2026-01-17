using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class NPC : MonoBehaviour
{
    int playerDetected;
    GameObject playerGameObject;
    public Vector2 directionalInput;
    float gravity;
    float maxJumpVelocity;
    float minJumpVelocity;
    float velocityXSmoothing;
    public float maxJumpHeight = 4;
    public float minJumpHeight = 1;
    public float timeToJumpApex = .4f;
    float accelerationTimeAirborne = .2f;
    float accelerationTimeGrounded = .1f;
    float moveSpeed = 3;
    public bool faceRight = true;
    Controller2D controller;
    public Vector3 velocity;
    public Vector2 wallJumpClimb;
    public Vector2 wallJumpOff;
    public Vector2 wallLeap;
    public float wallSlideSpeedMax = 3;
    public float wallStickTime = .25f;
    float timeToWallUnstick;
    public float switchDirectionTimer = 0;
    bool wallSliding;
    int wallDirX;
    public bool onLadder = false;
    float ladderSpeed = 2f;
    public float ladderXPosition;

    public float[] localNPCPerimeter;
    float[] globalNPCPerimeter;

    private float previousTimeScale = 1f;   // new field to remember TS during dialog

    private List<int> remainingDialogs = new List<int> { 1, 2, 3, 4, 5 };
    private TextMeshPro tmp;   // cache the component

    void Awake()
    {
        tmp = GetComponentInChildren<TextMeshPro>();
    }

    // Start is called before the first frame update
    public void Start()
    {
        globalNPCPerimeter = new float[localNPCPerimeter.Length]; // Each NPC needs to have boundaries
        for (int i = 0; i < localNPCPerimeter.Length; i++)
        {
            globalNPCPerimeter[i] = localNPCPerimeter[i] + transform.position.x;
        }
        playerDetected = -1;
        directionalInput.x = -1;
        gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
        controller = GetComponent<Controller2D>();
    }

    // Update is called once per frame
    void Update()
    {
        // unable to speak to NPC when coming down from a ladder without moving horizonally
        // float oldTimeScale = 1; // this used to be in place but I am trying to replace it with previousTimeScale
        if (playerDetected == 1 && playerGameObject != null)
        {
            var player = playerGameObject.GetComponent<Player>();

            // Start dialog
            if (Input.GetKeyDown(KeyCode.LeftControl) &&
                Time.timeScale != 0 &&
                (player.controller.collisions.below || player.onLadder))
            {
                player.dialog = true;
                Time.timeScale = 0f;

                // Set NPC text based on name
                if (this.gameObject.name == "George")
                {
                    int dialog = Random.Range(1, 5);
                    switch (dialog)
                    {
                        case 1:
                            gameObject.GetComponentInChildren<TextMeshPro>().text = "Hello!";
                            break;
                        case 2:
                            gameObject.GetComponentInChildren<TextMeshPro>().text = "Yo!";
                            break;
                        case 3:
                            gameObject.GetComponentInChildren<TextMeshPro>().text = "Test!";
                            break;
                        default:
                            gameObject.GetComponentInChildren<TextMeshPro>().text = "Go away!";
                            break;
                    }
                }
                else if (this.gameObject.name == "Josephine")
                {
                    gameObject.GetComponentInChildren<TextMeshPro>().text = "My name is Josephine";
                }
                else if (this.gameObject.name == "GreyCat")
                {
                    AudioSource audio = GetComponent<AudioSource>();
                    audio.Play();
                    Talk();
                }
            }
            // End dialog
            else if (Input.GetKeyDown(KeyCode.LeftControl) && player.dialog)
            {
                player.dialog = false;
                gameObject.GetComponentInChildren<TextMeshPro>().text = "";

                // Only resume time if pause menu is NOT open
                PauseMenu pauseMenu = FindObjectOfType<PauseMenu>();
                if (pauseMenu == null || !pauseMenu.IsPaused)
                {
                    Time.timeScale = 1f;
                }
            }
        }

        // Movement logic
        if (directionalInput.x == 1)
        {
            faceRight = true;
        }
        else if (directionalInput.x == -1)
        {
            faceRight = false;
        }

        CalculateVelocity();
        NPCIntelligence();
        controller.Move(velocity * Time.deltaTime, directionalInput);

        if (controller.collisions.above || controller.collisions.below)
        {
            if (controller.collisions.slidingDownMaxSlope)
            {
                velocity.y += controller.collisions.slopeNormal.y * -gravity * Time.deltaTime;
            }
            else
            {
                velocity.y = 0;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerDetected = 1;
            playerGameObject = collision.gameObject;
            playerGameObject.gameObject.GetComponent<Player>().collisionNPC = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerDetected = 0;
            playerGameObject.gameObject.GetComponent<Player>().collisionNPC = false;
        }
    }

    public void OnJumpInputDown()
    {
        if (controller.collisions.below)
        {
            if (controller.collisions.slidingDownMaxSlope)
            {
                if (directionalInput.x != -Mathf.Sign(controller.collisions.slopeNormal.x))
                {
                    velocity.x = -wallDirX * wallLeap.x;
                    velocity.y = wallLeap.y;
                }
                else if (wallDirX == -directionalInput.x)
                {
                    velocity.x = -wallDirX * wallJumpClimb.x;
                    velocity.y = wallJumpClimb.y;
                }
                else
                {
                    // not jumping against max slope
                    velocity.y = maxJumpVelocity * controller.collisions.slopeNormal.y;
                    velocity.x = maxJumpVelocity * controller.collisions.slopeNormal.x;
                }
            }
            else
            {
                velocity.y = maxJumpVelocity;
            }
        }
    }

    public void OnJumpInputUp()
    {
        if (velocity.y > minJumpVelocity)
        {
            velocity.y = minJumpVelocity;
        }
    }

    void HandleWallSliding()
    {
        wallDirX = (controller.collisions.left) ? -1 : 1;
        wallSliding = false;
        if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && velocity.y < 0)
        {
            wallSliding = true;

            if (velocity.y < -wallSlideSpeedMax)
            {
                velocity.y = -wallSlideSpeedMax;
            }

            if (timeToWallUnstick > 0)
            {
                velocityXSmoothing = 0;
                velocity.x = 0;

                if (directionalInput.x != wallDirX && directionalInput.x != 0)
                {
                    timeToWallUnstick -= Time.deltaTime;
                }
                else
                {
                    timeToWallUnstick = wallStickTime;
                }
            }
            else
            {
                timeToWallUnstick = wallStickTime;
            }
        }
    }

    void CalculateVelocity()
    {
        float targetVelocityX = directionalInput.x * moveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
        if (onLadder && controller.collisions.below && directionalInput.x != 0)
        {
            onLadder = false;
        }
        if (onLadder)
        {
            velocity.y = ladderSpeed * directionalInput.y - 0.001f;
            velocity.x = 0;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }
    }


    void NPCIntelligence()
    {
        if (controller.collisions.right && !controller.collisions.climbingSlope)
        {
            switchDirectionTimer += Time.deltaTime;
            if (switchDirectionTimer > 1)
            {
                directionalInput.x = -1;
                switchDirectionTimer = 0;
            }
        }
        else if (controller.collisions.left && !controller.collisions.climbingSlope)
        {
            switchDirectionTimer += Time.deltaTime;
            if (switchDirectionTimer > 1)
            {
                directionalInput.x = 1;
                switchDirectionTimer = 0;
            }
        }
        else if (transform.position.x < Mathf.Min(globalNPCPerimeter))
        {
            directionalInput.x = 1;
        }
        else if (transform.position.x > Mathf.Max(globalNPCPerimeter))
        {
            directionalInput.x = -1;
        }
        else
        {
            switchDirectionTimer = 0;
        }
    }
    void OnDrawGizmos()
    {
        if (localNPCPerimeter != null)
        {
            Gizmos.color = Color.yellow;
            float size = .3f;

            for (int i = 0; i < localNPCPerimeter.Length; i++)
            {
                float globalNPCPerimeterPos = (Application.isPlaying) ? globalNPCPerimeter[i] : localNPCPerimeter[i] + transform.position.x;
                Gizmos.DrawLine(new Vector2(globalNPCPerimeterPos, transform.position.y) - Vector2.up * size, new Vector2(globalNPCPerimeterPos, transform.position.y) + Vector2.up * size);
                Gizmos.DrawLine(new Vector2(globalNPCPerimeterPos, transform.position.y) - Vector2.left * size, new Vector2(globalNPCPerimeterPos, transform.position.y) + Vector2.left * size);
            }
        }
    }

    public void Talk()
    {
        // If we've used all 5, reset the list
        if (remainingDialogs.Count == 0)
        {
            remainingDialogs = new List<int> { 1, 2, 3, 4, 5 };
        }

        // Pick a *random index* into the remaining list
        int randomIndex = Random.Range(0, remainingDialogs.Count);
        int dialog = remainingDialogs[randomIndex];

        // Remove this dialog so it won't be picked again until reset
        remainingDialogs.RemoveAt(randomIndex);

        switch (dialog)
        {
            case 1:
                tmp.text = "Meow!";
                break;
            case 2:
                tmp.text = "Moo! Cough! Meow!";
                break;
            case 3:
                tmp.text = "Meow Meow Meowy Meow Meow!";
                break;
            case 4:
                tmp.text = "Meow Meow!";
                break;
            case 5:
                tmp.text =
                    "There is a secret in the level with a ladder...\nWhy am I not telling you more?\nWhere is the fun in that?";
                break;
        }
    }
}