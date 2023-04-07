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
    private Transform playerTransform;
    //Player GroundTest
    float horizontalInput;
    float verticalInput;
    Vector2 dir;
    Vector2 dashDir;
    bool tryToDash;
    bool tryToJump;
    private void Awake()
    {
        playerTransform = this.transform;
    }

    void Update()
    {
        //TODO: Change All Variables depending on Inputs
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        if (Input.GetButton("Jump"))
        {
            tryToJump = true;
        }
        
        if (Input.GetButtonDown("Fire1"))
        {
            tryToDash = true;
            //TODO: Get dashDirection from Input
            Vector3 mousePosRaw = Input.mousePosition;
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(mousePosRaw);
            Vector2 playerpos = playerTransform.position;
            dashDir = (mousePos - playerpos);
            dashDir.Normalize();
            Debug.Log("dir: " + dashDir);
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
        if (tryToDash)
        { 
            if (CanDash())
            {
                playerMovement.GravityDash(dashDir);
            }
            tryToDash = false;
        }
        SnapToGround();
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
    private void SnapToGround()
    {
        Collider2D groundCollider = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, LayerMask.GetMask("Ground"));
        if (groundCollider != null)
        {
            groundCollider.
        }
    }
}