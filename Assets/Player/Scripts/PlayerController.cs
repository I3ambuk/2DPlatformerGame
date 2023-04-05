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
    PlayerMovement playerMovement;
    //Player CeilingTest
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
        horizontalInput = Input.GetButtonRaw("Horizontal");
        verticalInput = Input.GetButtonRaw("Vertical");
        
        tryToJump = Input.GetButtonDown("Jump");
        tryToDash = Input.GetButtonDown("");
        if (tryToDash)
        {
            //TODO: Get dashDirection from Input
            //dashdir = ...
        }
    }

    void FixedUpdate()
    {
        dir = Vector2.normalize(new Vector2(horizontalInput, verticalInput));
        if (CanMove())
        {
            playerMovement.Move(dir);
        }
        if (tryToJump && CanJump())
        {
            playerMovement.Jump();
        }
        if (tryToDash && CanDash())
        {
            playerMovement.GravityDash(dashDir) ;
        }
    }

    private bool CanMove()
    {
        return playerMovement.CanMove();//&& ...
    }

    private bool CanJump()
    {
        return playerMovement.CanJump(); //&& ...
    }
    private bool CanDash()
    {
        return playerMovement.CanDash();//&& ...
    }
}