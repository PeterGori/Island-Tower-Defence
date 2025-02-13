using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{

    [Header("Movement")]
    public static float HorizontalMovementInputDirection;
    public static float VerticalMovementInputDirection;
    private bool FacingRight = true;
    public static bool Crouching;
    public float MovementSpeed;
    public float CrouchDragCoefficient;
    private float CrouchTime;
    private float CrouchTimeCounter;
    public float NoInputDragCoefficient;
    
    [Header("Jumping")]
    public float MaxJumpForce;
    public float MinJumpForce;
    public float CoyoteTime;
    private float CoyoteTimeCounter;
    public float JumpBufferTime;
    private float JumpBufferCounter;
    
        
    [Header("Dashing")]
    [SerializeField] private float DashForce;
    [SerializeField] private float DashTime;
    [SerializeField] private float DashDamping;
    private Vector2 DashDirection;
    private bool Dashing;
    private bool CanDash = true;
    private bool DashInput = false;
    
    
    [Header("Sliding")]
    [SerializeField] private float SlideDragCoefficient;
    private bool Sliding = false;
    
    
    [Header("Surroundings Checks")]
    public static bool GroundedCheck;
    public Transform GroundCheck;
    public Vector2 GroundCheckRadius;
    public LayerMask TheGround;
    public static bool Grounded;
    public float GroundTime;
    private float GroundTimeCounter = 0;
    public Transform LeftWallCheck;
    public Transform RightWallCheck;
    public Vector2 WallCheckRadius;
    private static bool LeftWall;
    private static bool RightWall;
    
    
    [Header("Player Components")]
    private Rigidbody2D PlayerRb;
    public BoxCollider2D PlayerCollider;
    public BoxCollider2D CrouchCollider;
    public GameObject Player;
    
    
    [Header("Misc")]
    private float time;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PlayerRb = GetComponent<Rigidbody2D>();
        Player = GameObject.Find("Player");
    }

    // Update is called once per frame
    void Update()
    {
        CheckSurroundings();
        CheckInput();
        CheckMovementDirection();
    }

    void FixedUpdate()
    {
        ApplyMovementInput();
    }

    // Checks all Overlap Colliders around the player and outputs booleans for all of them.
    private void CheckSurroundings()
    {
        Sliding = (LeftWall || RightWall) && PlayerRb.linearVelocityY < 0;
        
        if (Physics2D.OverlapBox(GroundCheck.position, GroundCheckRadius, 0, TheGround)) GroundTimeCounter += Time.deltaTime;
        else
        {
            Grounded = false;
            GroundTimeCounter = 0;
        }
        if (GroundTimeCounter > GroundTime) Grounded = true;
        GroundedCheck = GroundTimeCounter > 0;
        
        LeftWall = Physics2D.OverlapBox(LeftWallCheck.position, WallCheckRadius, 0, TheGround);
        RightWall = Physics2D.OverlapBox(RightWallCheck.position, WallCheckRadius, 0, TheGround);
    }

    // Checks if the player is changing direction and flips the sprite if they are.
    private void CheckMovementDirection()
    {
        if (FacingRight && HorizontalMovementInputDirection < 0)
        {
            Flip();
        }
        else if (!FacingRight && HorizontalMovementInputDirection > 0)
        {
            Flip();
        }
    }
    
    private void CheckInput()
    {
        // Gets the horizontal input from the player for use in movement and checking if the sprite should be flipped
        HorizontalMovementInputDirection = Input.GetAxisRaw("Horizontal");
        VerticalMovementInputDirection = Input.GetAxisRaw("Vertical");
        
        // Checks if the player is grounded, if they are CoyoteTimeCounter is reset so the player can jump after walking off a ledge for a small window.
        if (Grounded)
        {
            CoyoteTimeCounter = CoyoteTime;
        }
        // If the player is not grounded, the CoyoteTimeCounter is decremented by Time.deltaTime to limit the window the player can jump after walking off a ledge.
        else
        {
            CoyoteTimeCounter -= Time.deltaTime;
        }
        
        // If the player presses the space key, the JumpBufferCounter is reset. Jump Buffering is used to allow the player to jump if they press the space key before they hit the ground.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            JumpBufferCounter = JumpBufferTime;
        }
        // The JumpBufferCounter is decremented by Time.deltaTime to limit the window the player can press space in before hitting the ground, to reduce accidental jumps.
        else
        {
            JumpBufferCounter -= Time.deltaTime;
        }
        
        // To implement variable jump height, if the player releases space while they are still ascending at a rate greater than the MinJumpForce, the player's y velocity is set to the MinJumpForce.
        if (Input.GetKeyUp(KeyCode.Space) && PlayerRb.linearVelocity.y > MinJumpForce)
        {
            PlayerRb.linearVelocity = new Vector2(PlayerRb.linearVelocity.x, MinJumpForce);
        }
        
        // Finally checks the JumpBufferCounter, CoyoteTimeCounter, Crouching Status to see if the player should be able to jump.
        if(JumpBufferCounter > 0 && CoyoteTimeCounter > 0f && !Crouching)
        {
            Jump();
            JumpBufferCounter = 0;
        }

        // Checks if the player is pressing the S key and if they are grounded, if they are the player crouches. This reduces movement speed and changes the hitbox of the player.
        if (Input.GetKey(KeyCode.S) && Grounded)
        {
            Crouch();
        }
        // If the player is not pressing the S key, the player stands up and the movement speed is reset to the walk speed.
        else
        {
            PlayerCollider.enabled = true;
            CrouchCollider.enabled = false;
            Crouching = false;
        }

        // Checks for if the player wants to dash
        DashInput = Input.GetKeyDown(KeyCode.LeftShift);

        // Checks for if the player can dash, and if they can does the dash
        if (DashInput && CanDash)
        {
            Dashing = true;
            CanDash = false;
            PlayerAnimator.Dash();
            DashDirection = new Vector2(HorizontalMovementInputDirection, 0);
            if (DashDirection == Vector2.zero)
            {
                DashDirection = FacingRight ? new Vector2(1, 0) : new Vector2(-1, 0);
            }

            StartCoroutine(StopDash());
        }

        // Resets the players ability to dash
        // TODO: Add Cooldown and UI Element for it
        if (Grounded)
        {
            CanDash = true;
        }
        
    }
    
    // Applies the movement input to the player's rigidbody using the MovementSpeed and MovementInputDirection.
    private void ApplyMovementInput()
    {
        // If the player is walking speed remains constant
        if (!Crouching && !Dashing && !Sliding && HorizontalMovementInputDirection != 0) PlayerRb.linearVelocity = new Vector2(MovementSpeed * HorizontalMovementInputDirection, PlayerRb.linearVelocity.y);
        // Applies dashing movement speed
        else if (Dashing) PlayerRb.linearVelocity = DashDirection.normalized * DashForce;
        // Applies sliding drag
        else if (Sliding) PlayerRb.linearVelocity = new Vector2(MovementSpeed * HorizontalMovementInputDirection, PlayerRb.linearVelocity.y * SlideDragCoefficient);
        // Applies crouch speed reduction
        else if (Crouching)
            PlayerRb.linearVelocity = Mathf.Abs(PlayerRb.linearVelocityX) > 0.2
                ? new Vector2(PlayerRb.linearVelocityX * CrouchDragCoefficient, PlayerRb.linearVelocity.y)
                : new Vector2(0, PlayerRb.linearVelocity.y);
        else PlayerRb.linearVelocity = new Vector2(PlayerRb.linearVelocityX * NoInputDragCoefficient, PlayerRb.linearVelocity.y); 
    }

    // Makes the player jump by setting the player's y velocity to the MaxJumpForce and playing the jump animation.
    private void Jump()
    {
        PlayerRb.linearVelocity = new Vector2(PlayerRb.linearVelocity.x, MaxJumpForce);
        PlayerAnimator.Jump();
    }

    // Makes the player crouch by changing the player's hitbox and reducing the player's movement speed.
    private void Crouch()
    {
        PlayerCollider.enabled = false;
        CrouchCollider.enabled = true;
        Crouching = true;
    }

    // Stops the dash after the allotted DashTime, then damps the velocity to prevent a 'brickwall' or 'floating' effect
    private IEnumerator StopDash()
    {
        yield return new WaitForSeconds(DashTime);
        Dashing = false;
        PlayerRb.linearVelocity *= DashDamping;
    }

    // Flips the player sprite when the player changes direction.
    private void Flip()
    {
        FacingRight = !FacingRight;
        transform.Rotate(0,180,0);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(GroundCheck.position, GroundCheckRadius);
        Gizmos.DrawWireCube(LeftWallCheck.position, WallCheckRadius);
        Gizmos.DrawWireCube(RightWallCheck.position, WallCheckRadius);
    }
}
