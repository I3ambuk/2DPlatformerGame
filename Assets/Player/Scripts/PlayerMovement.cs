using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Kümmert sich um die Bewegung des Spielers
 * Logik der Bewegungen
 * Bewegungen werden vom PlayerController initialisiert
 */

public class PlayerMovement : MonoBehaviour
{

	[SerializeField] private float speed;

	private Rigidbody2D rigidbody;

	private Vector2 up;
	private Vector2 left;
	private float gravityScale;

	//Initialize Variabless
	void Awake()
    {
		rigidbody = GetComponent<Rigidbody2D>();
		up = new Vector2(0, 1);
		left = new Vector2(-1, 0);
		gravityScale = 9.81f;
	}

	void FixedUpdate()
    {
		//Change Player position in the world, depending on players velocity
		
		//Jump Logic
		//Dash Logic
    }

	/**
	 * public Movement Methods, Called from Player Controller, activates Movement in FixedUpdate()
	*/
	public void Rotate(Vector2 newUpvector)
	{
		//TODO: Implement Rotation Behavior of the Player
	}

	public void Move(Vector2 dir) //--> Changes Player Velocity
	{
		//TODO: Move Left/Right(from Player view) depending what is closest to the given direction
		//For Example: The Direction is Up in World View, but the player runs on left Wall, so the here the player should run left from players view.
		if (dir.magnitude == 0)
        {
			rigidbody.velocity = Vector2.zero;
			return;
        }

		float angleToUp = Vector2.SignedAngle(dir, this.up);
		switch (angleToUp)
        {
			case float n when (n >= 45 && n < 135):
				//MoveRight
				rigidbody.velocity = -this.left * this.speed;
				break;
			case float n when (n >= 135 && n < -135):
				//MoveDown
				break;
			case float n when (n >= -135 && n < -45):
				//MoveLeft
				rigidbody.velocity = this.left * this.speed;
				break;
			default:
				//MoveUp
				break;
        }
	}
	public void Jump()
	{
		//Implement Jump Event
	}
	public void GravityDash(Vector2 dashDir)
    {
		//TODO: Implement GravitDash Event
		//Change Rotation
		//Change Player Velocity
		//Change Player Gravity Scale
    }


	public bool CanMove()
	{
		return true;
	}
	public bool CanJump()
	{
		return true;
	}
	public bool CanDash()
    {
		return true;
    }

}
