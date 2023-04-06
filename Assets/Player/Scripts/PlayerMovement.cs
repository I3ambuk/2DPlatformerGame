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
	[SerializeField] private float jumpheight;


	private Rigidbody2D rb;

	private Vector2 up;
	private Vector2 left;
	private Vector2 moveVelocity = Vector2.zero;
	private Vector2 fallVelocity = Vector2.zero;
	private float jumpVelocityScale;
	private float defaultGravityScale;
	private bool isJumping;
	private Vector2 maxHeightPoint;
	private Vector2 groundLevelPoint;

	//Initialize Variabless
	void Awake()
    {
		rb = GetComponent<Rigidbody2D>();
		up = new Vector2(0, 1);
		left = new Vector2(-1, 0);
		defaultGravityScale = 9.81f;

		jumpVelocityScale = Mathf.Sqrt(2 * jumpheight * defaultGravityScale);
	}

	void FixedUpdate()
    {
		//Jump Logic
		if (isJumping)
        {
			if (rb.velocity.sqrMagnitude != 0 && Vector2.Angle(rb.velocity, this.up) > 90)
            {
				//Player is moving up, lower Gravity 
				fallVelocity -= defaultGravityScale * Time.deltaTime * this.up;
			} else
            {
				isJumping = false;
            }
		} else
        {
			//Gravity when falling: TODO: Gravity does not working
			fallVelocity -= defaultGravityScale * 3 * Time.deltaTime * this.up;
		}
		rb.velocity = fallVelocity + moveVelocity;
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
		if (dir.sqrMagnitude == 0)
        {
			//BUG: Setzen auf 0 verhindert fallen.:(
			moveVelocity = Vector2.zero;
			return;
        }

		float angleToUp = Vector2.SignedAngle(dir, this.up);
		switch (angleToUp)
        {
			case float n when (n >= 45 && n < 135):
				//MoveRight
				moveVelocity = -this.left * this.speed;
				break;
			case float n when (n >= 135 && n < -135):
				//MoveDown
				break;
			case float n when (n >= -135 && n < -45):
				//MoveLeft
				moveVelocity = this.left * this.speed;
				break;
			default:
				//MoveUp
				break;
        }
	}
	public void Jump()
	{
		//Implement Jump Event
		isJumping = true;
		fallVelocity = this.up * jumpVelocityScale;
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
