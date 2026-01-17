using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Controller2D))]
public class Ennemy : MonoBehaviour
{
	public float maxJumpHeight = 4;
	public float minJumpHeight = 1;
	public float timeToJumpApex = .4f;
	float accelerationTimeAirborne = .2f;
	float accelerationTimeGrounded = .1f;
	public float moveSpeed = 3;
	public int health = 1;
	public int armor = 0;
	public float damageCoolDown = 0.3f;
	public float damageDecreasingCoolDown = 0;
	public int damage = 1;
	public bool overlapDoor = false;
	public bool onLadder = false;
	float ladderSpeed = 2f;
	public float ladderXPosition;
	public bool faceRight = true;
	public int bumpResistance = -1;
	
    public float[] localEnnemyPerimeter;
    float[] globalEnnemyPerimeter;

	public Vector2 wallJumpClimb;
	public Vector2 wallJumpOff;
	public Vector2 wallLeap;

	public float wallSlideSpeedMax = 3;
	public float wallStickTime = .25f;
	float timeToWallUnstick;
	public int bumpNumber = 0;
	public float initRepeatBumpDelay = 1f;
	public float repeatBumpDelay = 0.5f;
	public float switchDirectionTimer = 0;

	float gravity;
	float maxJumpVelocity;
	float maxSecondJumpVelocity;
	float minJumpVelocity;
	float minSecondJumpVelocity;
    public Vector3 velocity;
	float velocityXSmoothing;
	public float ennemyPlayerCollisionsTrigger = 0;

	Controller2D controller;

	public Vector2 directionalInput;
	bool wallSliding;
	int wallDirX;
	public bool doubleJump;
	public bool secondJump;
	GameObject punch2;
	float punchDelay = 0;
	float initPunchDelay = 0.1f;
	int punchDirection = 1;

    [Header("Respawn")]
    public bool canRespawn = true;
    public float respawnDelay = 3f;

    Vector3 spawnPosition;
    Quaternion spawnRotation;

    Collider2D[] colliders;
    SpriteRenderer[] renderers;
    Behaviour[] behaviours;
    Rigidbody2D rb;

    bool isDead;
	int startHealth;

    // Two parent AudioSources:
    [Header("Audio (parent)")]
    public AudioSource deathSource;   // plays when enemy dies
    public AudioSource hitSource;     // plays when enemy takes a hit

	void Start()
	{
		globalEnnemyPerimeter = new float[localEnnemyPerimeter.Length]; // Each Ennemy needs to have boundaries
        for (int i = 0; i < localEnnemyPerimeter.Length; i++)
        {
            globalEnnemyPerimeter[i] = localEnnemyPerimeter[i] + transform.position.x;
        }
		controller = GetComponent<Controller2D>();
		directionalInput.x = -1;
		gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
		maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
		minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
		maxSecondJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex * 0.8f;
		minSecondJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight) * 0.8f;
	}

	void Update()
	{
		if (isDead) return;

		if (directionalInput.x == 1)
		{
			faceRight = true;
		}
		else if (directionalInput.x == -1)
		{
			faceRight = false;
		}
		CalculateVelocity();
		HandleWallSliding();
		CalculatePunchVelocity();
		if (ennemyPlayerCollisionsTrigger != 0)
		{
			EnnemyPlayerCollisions(ennemyPlayerCollisionsTrigger);
		}
		EnnemyIntelligence();
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

		if (damageDecreasingCoolDown > 0)
		{
			damageDecreasingCoolDown -= Time.deltaTime;
		}
		else if (damageDecreasingCoolDown < 0)
		{
			damageDecreasingCoolDown = 0;
		}
		if (health <= 0)
		{
			Death();
		}

		if (controller.collisions.below || wallSliding)
		{
			secondJump = true;
		}
	}

	public void OnJumpInputDown()
	{
		if (onLadder)
		{
			onLadder = false;
			velocity.y = maxJumpVelocity;
		}
		if (wallSliding)
		{
			if (wallDirX == directionalInput.x)
			{
				velocity.x = -wallDirX * wallJumpClimb.x;
				velocity.y = wallJumpClimb.y;
			}
			else if (directionalInput.x == 0)
			{
				velocity.x = -wallDirX * wallJumpOff.x;
				velocity.y = wallJumpOff.y;
			}
			else
			{
				velocity.x = -wallDirX * wallLeap.x;
				velocity.y = wallLeap.y;
			}
		}
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
					velocity.y = maxJumpVelocity * controller.collisions.slopeNormal.y;
					velocity.x = maxJumpVelocity * controller.collisions.slopeNormal.x;
				}
			}
			else
			{
				velocity.y = maxJumpVelocity;
			}
		}
		else if (doubleJump == true && secondJump == true && !wallSliding)
		{
			velocity.y = maxSecondJumpVelocity;
			secondJump = false;
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
	public void EnnemyPlayerCollisions(float playerVelocityX)
	{
		controller.collisions.playerEnnemyCollision = "ongoing";

		if (velocity.x > playerVelocityX)
		{
			controller.collisions.ennemyPosition = "left";
		}
		else
		{
			controller.collisions.ennemyPosition = "right";
		}
		ennemyPlayerCollisionsTrigger = 0;
	}

	void CalculateVelocity()
	{
		if (controller.collisions.punch2Position == "right")
		{
			//				print("Right");
			velocity.x = 4f * -bumpResistance;
		}
		else if (controller.collisions.punch2Position == "left")
		{
			//				print("Left");
			velocity.x = -4f * -bumpResistance;
		}
		else if (controller.collisions.punch2Position == "top")
		{
			//				print("Top");
			velocity.y = -4f * -bumpResistance;
		}
		else if (controller.collisions.punch2Position == "bottom")
		{
			//				print("Bottom");
			velocity.y = 4f * -bumpResistance;
		}
		else if (controller.collisions.ennemyPlayerCollision == "ongoing")
		{
			if (controller.collisions.ennemyPosition == "right")
			{
//				print("Right");
				velocity.x = 4f;
				bumpNumber += 1;
				controller.collisions.ennemyPlayerCollision = "beingPushedBack";
			}
			else if (controller.collisions.ennemyPosition == "left")
			{
//				print("Left");
				velocity.x = -4f;
				bumpNumber += 1;
				controller.collisions.ennemyPlayerCollision = "beingPushedBack";
			}
			else if (controller.collisions.ennemyPosition == "top")
			{
				velocity.y = 10f;
				controller.collisions.ennemyPlayerCollision = "no";
			}
			else if (controller.collisions.ennemyPosition == "bottom")
			{
				// print("Bottom");
				if (!controller.collisions.below) // If we are standing on the ground or slope, we cannot go down and need to go to the opposite direction that we are going.
				{
					velocity.y = -4f;
				}
				else
				{
					if (controller.goingRight > 0)
					{
						velocity.x = -20f; // to adjust when bumping into ennemy from below (ennemy moving towards player or opposite from player.
					}
					else if (controller.goingRight < 0)
					{
						velocity.x = 20f; // to adjust when bumping into ennemy from below (ennemy moving towards player or opposite from player.
					}
				}
				controller.collisions.ennemyPlayerCollision = "no";
			}
			if (bumpNumber > 0)
			{
//				print("Bump Number = " + bumpNumber);
				if (bumpNumber > 1)
				{
					directionalInput.x = -directionalInput.x;
				}
				repeatBumpDelay = initRepeatBumpDelay;
			}
		}
		else
		{
			if (bumpNumber > 0)
			{
//				print(controller.collisions.ennemyPosition);
				repeatBumpDelay -= Time.deltaTime;
				if (repeatBumpDelay <= 0)
				{
					repeatBumpDelay = initRepeatBumpDelay;
					bumpNumber = 0;
					controller.collisions.ennemyPlayerCollision = "no";
				}
			}
			float targetVelocityX = directionalInput.x * moveSpeed;
			velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
		}

		if (onLadder && controller.collisions.below && directionalInput.x != 0)
		{
			onLadder = false;
		}
		if (onLadder)
		{
			velocity.y = ladderSpeed * directionalInput.y - 0.001f;
		//	this.transform.position = new Vector2(ladderXPosition, this.transform.position.y);
			velocity.x = 0;
		}
		else
		{
			velocity.y += gravity * Time.deltaTime;
		}
	}

	void EnnemyIntelligence()
	{
		if (controller.collisions.right || controller.collisions.left /*&& !controller.collisions.climbingSlope*/)
		{
			switchDirectionTimer += Time.deltaTime;
			if (switchDirectionTimer > 1)
			{
				directionalInput.x *= -1;
				switchDirectionTimer = 0;
			}
		}
		else if (localEnnemyPerimeter.Length != 0){ // Each ennemy is supposed to have a perimeter. This is just so that the ennemy does not end up facing only one direction if we forget to assign a perimeter to an ennemy.
			if (transform.position.x < Mathf.Min(globalEnnemyPerimeter))
			{
				directionalInput.x = 1;
			}
			else if (transform.position.x > Mathf.Max(globalEnnemyPerimeter))
			{
				directionalInput.x = -1;
			}
		}
		else
		{
			switchDirectionTimer = 0;
		}
	}

    void OnDrawGizmos()
    {
        if (localEnnemyPerimeter != null)
        {
            Gizmos.color = Color.yellow;
            float size = .3f;

            for (int i = 0; i < localEnnemyPerimeter.Length; i++)
            {
                float globalEnnemyPerimeterPos = (Application.isPlaying) ? globalEnnemyPerimeter[i] : localEnnemyPerimeter[i] + transform.position.x;
                Gizmos.DrawLine(new Vector2(globalEnnemyPerimeterPos, transform.position.y) - Vector2.up * size, new Vector2(globalEnnemyPerimeterPos, transform.position.y) + Vector2.up * size);
                Gizmos.DrawLine(new Vector2(globalEnnemyPerimeterPos, transform.position.y) - Vector2.left * size, new Vector2(globalEnnemyPerimeterPos, transform.position.y) + Vector2.left * size);
            }
        }
    }

	public void Punch()
	{
		if (!punch2)
		{
			GameObject punch = this.transform.GetChild(0).gameObject;
			punch2 = Instantiate(punch, new Vector3(this.transform.localPosition.x, this.transform.localPosition.y, this.transform.localPosition.z), Quaternion.identity);
			punch2.transform.parent = gameObject.transform;
			punch2.transform.localScale = new Vector3(0.5f, 0.5f, 0);
			punch2.SetActive(true);
			punchDelay = initPunchDelay;
			if (faceRight)
			{
				punchDirection = 1;
			}
			else
			{
				punchDirection = -1;
			}
		}
	}

	void CalculatePunchVelocity()
	{
		if (punchDelay > 0)
		{
			punchDelay -= Time.deltaTime;
			punch2.transform.localPosition = new Vector3(punch2.transform.localPosition.x + 0.2f * punchDirection, punch2.transform.localPosition.y, punch2.transform.localPosition.z);
			if (punchDelay <= 0)
			{
				Destroy(punch2);
			}
		}
	}

    void Awake()
    {
		spawnPosition = transform.position;
		spawnRotation = transform.rotation;

		startHealth = health;   // store initial health
		rb = GetComponent<Rigidbody2D>();

		colliders = GetComponentsInChildren<Collider2D>(true);
		renderers = GetComponentsInChildren<SpriteRenderer>(true);

		behaviours = GetComponents<Behaviour>();

        // Auto-wire if not set in inspector
        if (deathSource == null || hitSource == null)
        {
            var sources = GetComponentsInParent<AudioSource>();

            if (sources.Length > 0 && deathSource == null)
                deathSource = sources[0];

            if (sources.Length > 1 && hitSource == null)
                hitSource = sources[1];
        }

        if (deathSource == null)
            Debug.LogWarning($"[Ennemy] No death AudioSource found in parent of {name}.");

        if (hitSource == null)
            Debug.LogWarning($"[Ennemy] No hit AudioSource found in parent of {name}.");
    }

    public void PlayHitSound()
    {
        if (hitSource != null && hitSource.clip != null)
        {
            hitSource.PlayOneShot(hitSource.clip);
        }
    }

    public void Death()
    {
        // Play death sound from parent
        if (deathSource != null && deathSource.clip != null)
        {
            deathSource.PlayOneShot(deathSource.clip);
        }

        if (isDead) return;   // prevents multiple deaths looping
        isDead = true;

        if (canRespawn)
        {
            StartCoroutine(RespawnCoroutine());
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void SetAlive(bool alive)
    {
        // visuals
        foreach (var r in renderers) r.enabled = alive;

		foreach (var r in renderers)
		{
			r.enabled = alive;
			r.gameObject.SetActive(true); // ensure the sprite child object is active
		}
        // collisions
        foreach (var c in colliders) c.enabled = alive;

        // stop/allow motion
        if (rb != null)
        {
            rb.simulated = alive;
            if (alive)
            {
                rb.velocity = Vector2.zero; // NOT linearVelocity
                rb.angularVelocity = 0f;
            }
        }

        // IMPORTANT: don't disable this script or the coroutine will stop
        foreach (var b in behaviours)
        {
            if (b == this) continue;
            b.enabled = alive;
        }
    }
    IEnumerator RespawnCoroutine()
    {
        // "hide" enemy properly (no sprite, no collider)
        SetAlive(false);

        yield return new WaitForSeconds(respawnDelay);

        // reset position/state
        transform.position = spawnPosition;
        transform.rotation = spawnRotation;

		health = startHealth;
        isDead = false;

        SetAlive(true);
    }
}