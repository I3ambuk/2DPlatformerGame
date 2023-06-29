using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

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
 * --> TODO: implementiere rutschen auf schrägen oberflächen per hand, da code in die unity physik ainegreift.(hinzufügen von rutsch animation?)
 * 
 * TODO: MERGE CONFLICT fehlerhaft! Überprüfen! Sliding funktioniert nicht richtig, Rest der steuerung an sich schon glaub
 * TODO: Steuerung des Dashes anpassen, dass sowohl für controller als auch tastatur angenehm ist !! wahrscheinlich Refactoring notwendig um die Tastenbelegung austauschbar zu machen
 * TODO: Geschwindigkeit bei kollision mit decke wand beachten/ändern. BUG: springen gegen decke wirkt wie schweben, da kollision mit decke nicht beachtet wird
 * TDOD: Cached jumping, sodass man auch springt, kurz nachdem man den boden verlassen hat, bzw. springt sobald man aufkommt obwohl man noch in der luft war zu zeitpunkt des drückens.
 * TODO: Ausdauer Mechanik hinzufügen, sodass Spieler bei verbrauchter Ausdauer zu Boden fällt.
 * TODO: Refaktorisieren wo es machbar ist. Was sollte nach Außen sichtbar sein? Was muss in der UI geändert werden? Wie wird UI benachrichtigt
 * ?
 */

public class PlayerController : MonoBehaviour
{
    // Player Components
    [SerializeField] private Transform ceilingCheck;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.02f;
    [SerializeField] private Animator animator;

    private Rigidbody2D rb;
    private Transform playerTransform;
    private Vector2 up;
    private PlayerControls controls;

    //Input
    private Vector2 movementDir = Vector2.zero;
    private Vector2 dashDir = Vector2.zero;

    //Status
    private bool isGrounded;
    private bool isRising;
    private bool isFalling;
    private bool isSliding;
    private bool isDashing;
    private bool isAiming;

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
    [SerializeField] private float slideSpeed = 5f;
    private float dashTimer = 0f;
    private float dashDuration = 0.2f;
    private float jumpTimer = 0f;
    private float timerPlayerCanStillJump = 0f;

    private enum Control { KeyboardAndMouse, Gamepad};
    private Control currentControl;

    private void Awake()
    {
        controls = new PlayerControls();

        controls.Gameplay.Jump.performed += ctx =>
        {
            jumpTimer = 0.2f;
            Jump();
        };
        controls.Gameplay.Dash.performed += ctx => {
            //Holds Movement, while aiming for Dash
            Move(Vector2.zero);
            isAiming = true;
            //Sets currentControl, depending on device name
            if (controls.Gameplay.Dash.activeControl.device.displayName == "Mouse")
            {
                currentControl = Control.KeyboardAndMouse;
            } else
            {
                currentControl = Control.Gamepad;
            }
        };
        controls.Gameplay.Dash.canceled += ctx =>
        {
            isAiming = false;
            GravityDash();
        };
        controls.Gameplay.Horizontal.performed += ctx => movementDir.x = ctx.ReadValue<float>();
        controls.Gameplay.Horizontal.canceled += ctx => movementDir.x = 0f;
        controls.Gameplay.Vertical.performed += ctx => movementDir.y = ctx.ReadValue<float>();
        controls.Gameplay.Vertical.canceled += ctx => movementDir.y = 0f;
    }
    private void OnEnable()
    {
        controls.Gameplay.Enable();
    }
    private void OnDisable()
    {
        controls.Gameplay.Disable();
    }
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
        movementDir.Normalize();
        UpdateAnimation();
    }

    void FixedUpdate()
    {
        //up = transform.up;
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
        
        jumpTimer = jumpTimer >= 0? jumpTimer - Time.deltaTime : 0f;

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

        Collider2D feet = groundCheck.gameObject.GetComponent<Collider2D>();
        List<Collider2D> collider = new List<Collider2D>();
        ContactFilter2D filter = new ContactFilter2D();
        feet.OverlapCollider(filter, collider);
        List<Collider2D> groundCollider = new List<Collider2D>();
        collider.ForEach(c =>
        {
            if (c.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                groundCollider.Add(c);
                
            }
        });
        if (groundCollider.Count > 0)
        {
            isGrounded = true;
            RaycastHit2D[] hitResults = new RaycastHit2D[1]; // Array to store the RaycastHit2D
            int hitCount = feet.Raycast(-playerTransform.up, hitResults, 1f, LayerMask.GetMask("Ground")); //Raycast from feet pointing down

            if (hitCount > 0)
            {
                Vector2 groundNormal = hitResults[0].normal;
                Debug.Log("Normal Vector: " + groundNormal + ", Object: " + hitResults[0].transform.name);
                //Checks if the dash direction is similar to the ground normal, if so snap gravity to ground
                float slopeAngle = Vector2.Angle(groundNormal, up);
                float slopeAngleAbs = Mathf.Abs(Vector2.Angle(groundNormal, up));
                if (isDashing && slopeAngleAbs < 20)
                {
                    up = groundNormal;
                    playerTransform.rotation = Quaternion.LookRotation(playerTransform.forward, groundNormal);
                }
                //Check if the surface is steep, so the player will slide down the surface
                if (slopeAngleAbs >= 45)
                {
                    isSliding = true;
                }
                else
                {
                    isSliding = false;
                    //rotate player on not too steep surfaces
                    playerTransform.rotation = Quaternion.LookRotation(playerTransform.forward, groundNormal);
                }
            }
        } else
        {
            if (isGrounded)
            {
                timerPlayerCanStillJump = 0.2f; //TODO: MagicNumber!! (jumpTimer auch)
            } 
            isGrounded = false;
        }
        timerPlayerCanStillJump = timerPlayerCanStillJump >= 0 ? timerPlayerCanStillJump - Time.deltaTime : 0f;

        //Set player Velocity
        fallVelocity = isFalling || isRising || isSliding ? fallVelocity + gravityVector * Time.deltaTime : Vector2.zero;
        rb.velocity = moveVelocity + fallVelocity + dashVelocity;
        //Debug.Log("fall:" + fallVelocity + "\n dash:" + dashVelocity + "\n move:" + moveVelocity);

    }


    private void UpdateAnimation()
    {
        animator.SetBool(nameof(isRising), isRising);
        animator.SetBool(nameof(isFalling), isFalling);
        animator.SetBool(nameof(isDashing), isDashing);
        animator.SetBool(nameof(isGrounded), isGrounded);
        Vector2 right = playerTransform.right;

        float floatDir = moveVelocity == Vector2.zero ? 0f : (right - moveVelocity.normalized == Vector2.zero) ? 1f : -1f;
        animator.SetFloat("direction", floatDir);
        FlipAnimation(floatDir);
    }
    private void FlipAnimation(float floatDir)
    {
        if (floatDir != 0)
        {
            Vector3 newScale = animator.transform.localScale;
            newScale.x = floatDir * Mathf.Abs(newScale.x);
            animator.transform.localScale = newScale;
        }
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        //Checks if the feet hits the ground, if so snap player to the ground.
        if (collision.otherCollider.gameObject.name == groundCheck.name && collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            fallVelocity = Vector2.zero;

            /*//Checks if the dash direction is similar to the ground normal, if so snap gravity to ground
            Vector2 groundNormal = collision.GetContact(0).normal;
            float angle = Mathf.Abs(Vector2.Angle(groundNormal, up));
            if (isDashing && angle < 20)
            {
                up = groundNormal;
                playerTransform.rotation = Quaternion.LookRotation(playerTransform.forward, groundNormal);
            }

            if (angle < 45) //TODO:rotate gravitation only on dashing, but player always
            {
                playerTransform.rotation = Quaternion.LookRotation(playerTransform.forward, groundNormal);
            }

            //Check if the surface is steep, so the player will slide down the surface
            float slopeAngle = Vector2.Angle(groundNormal, up);
            int vz = slopeAngle < 0 ? -1 : 1;
            if (Mathf.Abs(slopeAngle) >= 45)
            {
                Vector2 groundLeft = Vector2.Perpendicular(groundNormal);
                fallVelocity = slideSpeed * vz * groundLeft;
                moveVelocity = Vector2.zero;
                isSliding = true;
                Debug.Log(fallVelocity);
            } else
            {
                isSliding = false;
            }*/

            //Checks if the player is pressing Jump or pressed Jump shortly before landing
            if (controls.Gameplay.Jump.IsPressed() || jumpTimer > 0)
            {
                Jump();
            }
        }
        //if player hits a ceiling, start falling
        if (collision.otherCollider.gameObject.name == ceilingCheck.name && collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Debug.Log("Hit Ceiling");
            fallVelocity = Vector2.zero;
        }
    }
    public void OnCollisionExit2D(Collision2D collision)
    {
        //Checks if the feet leave the ground, if so let the player fall.
        if (collision.otherCollider.gameObject.name == groundCheck.name && collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            playerTransform.rotation = Quaternion.LookRotation(playerTransform.forward, up);
            isSliding = false;
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

        float angleToUp = Vector2.SignedAngle(dir, playerTransform.up);
        switch (angleToUp)
        {
            case float n when (n >= 20 && n < 160):
                //MoveRight
                moveVelocity = playerTransform.right * this.speed;
                break;
            case float n when (n >= -160 && n < -20):
                //MoveLeft
                moveVelocity = -playerTransform.right * this.speed;
                break;
            default: break;
        }
    }
    private void Jump()
    {
        if (CanJump())
        {
            jumpTimer = 0f;
            Debug.Log("JUMP");
            //Implement Jump Event
            isRising = true;
            fallVelocity = up * jumpVelocityScale;
        }
    }
    private void GravityDash()
    {

        //Calc Dash Direction
        if (currentControl == Control.KeyboardAndMouse)
        {
            Vector3 mousePosRaw = Input.mousePosition;
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(mousePosRaw);
            Vector2 playerpos = playerTransform.position;
            bool dashDirSimilarToGravity = Mathf.Abs(Vector2.SignedAngle((mousePos - playerpos), -up)) < 10;
            dashDir = dashDirSimilarToGravity ? Vector2.zero : (mousePos - playerpos).normalized;
        }
        else if (currentControl == Control.Gamepad)
        {
            bool movementDirSimilarToGravity = Mathf.Abs(Vector2.SignedAngle(movementDir, -up)) < 10;
            dashDir = movementDirSimilarToGravity ? Vector2.zero : movementDir.normalized;
        }
        if (CanDash())
        {
            //DashEvent
            isDashing = true;
            up = -this.dashDir;
            playerTransform.rotation = Quaternion.LookRotation(playerTransform.forward, -this.dashDir);
            fallVelocity = Vector2.zero;
            dashTimer = dashDuration;
        }
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
        return !isAiming && !isSliding && (isGrounded || isRising || isFalling);
    }
    private bool CanJump()
    {
        return !isSliding && (isGrounded || timerPlayerCanStillJump > 0);
    }
    private bool CanDash()
    {
        bool dashDirValid = this.dashDir != Vector2.zero;
        return  dashDirValid && (isGrounded || isRising || isFalling);
    }

    public string getDirection()
    {
        return up.ToString();
    }
}