using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Kümmert sich um die Steuerung des Spielers
 * Empfängt Input und prüft ob Handlung des Inputs Möglich ist.
 * 
 * 
 * Diese Klasse kennt sowohl die Komponenten des Spielers, als auch die Objekte mit denen der Spieler interagiert/kollidiert
 */
/*
 *
 * BUG: Manchmal ändert sich die Orientierung des spielers ohne dash, nach fallen auf eine schräge kante 
 * ->TODO: Entkopple Gravitation von Spieler Orientierung: Gravitation nur bei dash rotieren(evtl auch nur wenn orientierung der Oberfläche ähnlich des Gravitationsvektor ist),
 * Spieler rotation immer bei Kollision mit Oberfläche um bspw. schräge Klippen zu laufen)
 * 
 * TODO: Steuerung des Dashes anpassen, dass sowohl für controller als auch tastatur angenehm ist !! wahrscheinlich Refactoring notwendig um die Tastenbelegung austauschbar zu machen
 * TODO: Ausdauer Mechanik hinzufügen, sodass Spieler bei verbrauchter Ausdauer zu Boden fällt.
 * TODO: Refaktorisieren wo es machbar ist. Was sollte nach Außen sichtbar sein? Was muss in der UI geändert werden? Wie wird UI benachrichtigt
 * ?
 */

public class PlayerController : MonoBehaviour
{
    //Player Components
    [SerializeField] private Transform ceilingCheck;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.02f;
    [SerializeField] private Animator animator;

    private Rigidbody2D rb;
    private Transform playerTransform;
    private Vector2 up;

    //Input
    private Vector2 movementDir = Vector2.zero;
    private Vector2 dashDir = Vector2.zero;
    private bool tryToDash;
    private bool tryToJump;

    //Status
    private bool isGrounded;
    private bool isRising;
    private bool isFalling;
    private bool isDashing;

    //Movement
    private Vector2 moveVelocity = Vector2.zero;
    private Vector2 fallVelocity = Vector2.zero;
    private Vector2 dashVelocity = Vector2.zero;
    private float jumpVelocityScale;
    [SerializeField] private float jumpheight;
    private float defaultGravityScale = 9.81f;
    [SerializeField] float gravityFallFactor;
    [SerializeField] float dashSpeed;
    [SerializeField] private float speed = 10f;
    private float dashTimer = 0f;
    private float dashDuration = 0.2f;
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerTransform = this.transform;
        up = playerTransform.up;
        jumpVelocityScale = Mathf.Sqrt(2 * jumpheight * defaultGravityScale);
        isFalling = !isGrounded && !isRising;
    }

    void Update()
    {
        //Change All Variables depending on Inputs
        movementDir.x = Input.GetAxisRaw("Horizontal");
        movementDir.y = Input.GetAxisRaw("Vertical");
        movementDir.Normalize();

        if (Input.GetButton("Jump"))
        {
            tryToJump = true;
        }
        
        if (Input.GetButtonDown("Fire1"))
        {
            tryToDash = true;
            Vector3 mousePosRaw = Input.mousePosition;
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(mousePosRaw);
            Vector2 playerpos = playerTransform.position;
            dashDir = (mousePos - playerpos);
            dashDir.Normalize();
            Debug.Log("dir: " + dashDir);
        }

        //Animation
        animator.SetBool("isRising", isRising);
        animator.SetBool("isFalling", isFalling);
        animator.SetBool("isDashing", isDashing);
        animator.SetBool("isGrounded", isGrounded);
        Vector2 right = playerTransform.right;
        float floatDir = moveVelocity == Vector2.zero ? 0f : (right - moveVelocity.normalized == Vector2.zero) ? 1f : -1f;
        animator.SetFloat("direction", floatDir);
        //FlipAnimation
        if (floatDir != 0)
        {
            Vector3 newScale = animator.transform.localScale;
            newScale.x = floatDir * Mathf.Abs(newScale.x);
            animator.transform.localScale = newScale;
        }
    }

    void FixedUpdate()
    {
        up = transform.up;
        //No isFalling = !isGrounded && !isRising, so that onLandEvent is possible after hitting Ground again
        if (!isGrounded && !isRising)
        {
            isFalling = true;
        }
        //Handle Input Events
        if (CanMove())
        {
            this.Move(movementDir);
        }

        if (tryToJump)
        {
            if (CanJump())
            {
                this.Jump();
            }
            tryToJump = false;
        }

        if (tryToDash)
        { 
            if (CanDash())
            {
                this.GravityDash(dashDir);
            }
            tryToDash = false;
        }
        Vector2 gravityVector = gravityFallFactor * defaultGravityScale * (-up);
        //Rising
        if (isRising)
        { 
            bool velocityGoesUpwards = fallVelocity.normalized == up;
            if (velocityGoesUpwards)
            {
                gravityVector = defaultGravityScale * (-up);
            }
            else
            {
                isRising = false;
                isFalling = true;
            }
        }
        //Falling
        if (isFalling)
        {
            if (isGrounded)
            {
                OnLandEvent();
            }
        }
        //Dashing boost
        dashVelocity = Vector2.zero;
        if (isDashing && dashTimer > 0)
        {
            dashVelocity = dashSpeed * dashDir;
            dashTimer -= Time.deltaTime;
        }
        //Set player Velocity
        fallVelocity = isGrounded? Vector2.zero : fallVelocity + gravityVector * Time.deltaTime;
        rb.velocity = moveVelocity + fallVelocity + dashVelocity;
        //Debug.Log("fall:" + fallVelocity + "\n dash:" + dashVelocity + "\n move:" + moveVelocity);
    }
    public void OnCollisionEnter2D(Collision2D collision)
    {
        //Checks if the feet hits the ground, if so snap player to the ground.
        if (collision.otherCollider.gameObject.name == groundCheck.name && collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = true;
            if (isDashing) //TODO:rotate gravitation only on dashing, but player always
            {
                playerTransform.rotation = Quaternion.LookRotation(playerTransform.forward, collision.GetContact(0).normal);
            }
        }
    }
    public void OnCollisionExit2D(Collision2D collision)
    {
        //Checks if the feet leave the ground, if so let the player fall.
        if (collision.otherCollider.gameObject.name == groundCheck.name && collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = false;
        }
    }

    //Movement Events
    public void Move(Vector2 dir)
    {
        if (dir.sqrMagnitude == 0)
        {
            moveVelocity = Vector2.zero;
            return;
        }

        float angleToUp = Vector2.SignedAngle(dir, this.up);
        switch (angleToUp)
        {
            case float n when (n >= 45 && n < 135):
                //MoveRight
                moveVelocity = playerTransform.right * this.speed;
                break;
            case float n when (n >= 135 && n < -135):
                //MoveDown
                break;
            case float n when (n >= -135 && n < -45):
                //MoveLeft
                moveVelocity = -playerTransform.right * this.speed;
                break;
            default:
                //MoveUp
                break;
        }
    }
    private void Jump()
    {
        Debug.Log("JUMP");
        //Implement Jump Event
        isRising = true;
        isGrounded = false;
        fallVelocity = up * jumpVelocityScale;
    }
    private void GravityDash(Vector2 dashDir)
    {
        //TODO: Implement GravityDash Event
        isDashing = true;
        isGrounded = false;
        playerTransform.rotation = Quaternion.LookRotation(playerTransform.forward, -dashDir);
        fallVelocity = Vector2.zero;
        dashTimer = dashDuration;
    }
    private void OnLandEvent()
    {
        Debug.Log("LAND!!");
        isFalling = false;
        isDashing = false;
        //TODO: Snap to Grounded Surface when landing, so that controlling the player is possible again
        Collider2D ground = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, LayerMask.GetMask("Ground"));
        // Allgemeiner, da Object evtl von mehreren Seiten begehbar, Oder Leveldesign sehr aufwendig da manuell
       // playerTransform.rotation = Quaternion.LookRotation(playerTransform.forward, groundNormal);
    }
    //Check Movement
    private bool CanMove()
    {
        return isGrounded || isRising || isFalling;
    }
    private bool CanJump()
    {
       
        return isGrounded;
    }
    private bool CanDash()
    {
        Debug.Log(isFalling);
        return isGrounded || isRising || isFalling;
    }

    public string getDirection()
    {
        return up.ToString();
    }
}