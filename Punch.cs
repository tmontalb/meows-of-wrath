using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Punch : MonoBehaviour
{
	public GameObject punch;
	public GameObject punch2;
	public GameObject punch3;
	GameObject ennemy;
	Player player;
	float initPunchDelay = 0.1f;
	public int punch2HorizontalDirection = 0;
	public int punch2VerticalDirection = 0;
	public int punchDamage = 1;
	// Start is called before the first frame update
	void Start()
    {

	}

	public void Spawn()
	{
		var player = transform.parent.GetComponentInParent<Player>();
		if (player.collisionNPC) return;

		// Spawn at the current punch object's position
		GameObject spawned = Instantiate(this.gameObject, transform.position, Quaternion.identity);

		// IMPORTANT: detach so it doesn't follow player
		spawned.transform.SetParent(null);

		// Make sure the spawned one is active
		spawned.SetActive(true);

		// Initialize movement on the spawned object
		var proj = spawned.GetComponent<Projectile>();
		if (proj != null)
		{
			Vector2 dir = GetPunchDirection(player);
			proj.Init(dir);
		}
		var hitbox = spawned.GetComponentInChildren<AttackHitbox>();
		if (hitbox != null)
		{
			hitbox.damage = player.punchDamage;

			// reset
			hitbox.dirX = 0;
			hitbox.dirY = 0;

			if (player.directionalInput.y > 0) hitbox.dirY = 1;
			else if (player.directionalInput.y < 0) hitbox.dirY = -1;
			else hitbox.dirX = player.faceRight ? 1 : -1;
		}
		// Optional: ensure the template object isn't visible
		// gameObject.SetActive(false);  // Only if this object is meant to be invisible in-game
	}

	Vector2 GetPunchDirection(Player player)
	{
		// Vertical attacks override horizontal
		if (player.directionalInput.y > 0)
			return Vector2.up;

		if (player.directionalInput.y < 0)
			return Vector2.down;

		// Horizontal
		return player.faceRight ? Vector2.right : Vector2.left;
	}

	public void Execute ()
	{	
		// Debug.Log(transform.parent.GetComponentInParent<Player>());
		if (transform.parent.GetComponentInParent<Player>().directionalInput.y == 1)
		{
			punch2VerticalDirection = 1;
		}
		else if (transform.parent.GetComponentInParent<Player>().directionalInput.y == -1)
		{
			punch2VerticalDirection = -1;
		}
		else if (transform.parent.GetComponentInParent<Player>().faceRight)
		{
			punch2HorizontalDirection = 1;
		}
		else
		{
			punch2HorizontalDirection = -1;
		}
	}

	// Update is called once per frame
	void Update()
    {
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.CompareTag("Ennemy"))
		{
			ennemy = collision.gameObject;
		}
	}
}


