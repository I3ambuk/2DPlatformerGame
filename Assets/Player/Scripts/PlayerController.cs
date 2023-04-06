using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Kümmert sich um die Steuerung des Spielers
 * Empfängt Input und prüft ob Handlung des Inputs Möglich ist
 * Entsprechnde Bewegung wird von PlayerMovement dann ausgeführt
 * 
 * Diese Klasse kennt sowohl die Komponenten des Spielers, als auch die Objekte mit denen der Spieler interagiert/kollidiert
 */

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Transform ceilingCheck;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.02f;
    //Player GroundTest
    float horizontalInput;
    float verticalInput;
    Vector2 dir;
    Vector2 dashDir;
    bool tryToDash;
    bool tryToJump;

    void Update()
    {
        //TODO: Change All Variables depending on Inputs
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        if (Input.GetButton("Jump"))
        {
            tryToJump = true;
        }
        
        //tryToDash = Input.GetButtonDown("");
        if (tryToDash)
        {
            //TODO: Get dashDirection from Input
            //dashdir = ...
        }
    }

    void FixedUpdate()
    {
        dir = new Vector2(horizontalInput, verticalInput).normalized;
        
        if (CanMove())
        {
            playerMovement.Move(dir);
        }
        if (tryToJump)
        {
            if (CanJump())
            {
                Debug.Log("Jump");
                playerMovement.Jump();
            }
            tryToJump = false;
        }
        if (tryToDash && CanDash())
        {
            playerMovement.GravityDash(dashDir);
        }

    }

    private bool CanMove()
    {
        return playerMovement.CanMove();//&& ...
    }

    private bool CanJump()
    {
        bool isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, LayerMask.GetMask("Ground"));
        return playerMovement.CanJump() && isGrounded; //&& ...
    }
    private bool CanDash()
    {
        return playerMovement.CanDash();//&& ...
    }
}