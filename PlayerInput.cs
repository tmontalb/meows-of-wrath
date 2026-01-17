using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Player))]
public class PlayerInput : MonoBehaviour
{

	Player player;
    Animator animator;

	void Start()
	{
		player = GetComponent<Player>();
		animator = GetComponentInChildren<Animator>();
	}

	void Update()
	{
		Vector2 directionalInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
		player.SetDirectionalInput(directionalInput);
		if (Input.GetAxisRaw("Horizontal") != 0)
		{
			animator.SetBool("Walking", true);
		}
		else
		{
			animator.SetBool("Walking", false);
		}
		if (Time.timeScale != 0){ // We don't want to register jump input when time is frozen, like when we talk to an NPC.
			if (Input.GetKeyDown(KeyCode.Space))
			{
				player.OnJumpInputDown();
			}
			if (Input.GetKeyUp(KeyCode.Space))
			{
				player.OnJumpInputUp();
			}
		}
		if (Input.GetKeyDown(KeyCode.LeftControl))
		{
			player.Attack();
		}
	}
}