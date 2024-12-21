using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{

    public static float MovementInputDirection;
    private float time;
    private bool FacingRight = true;
    public static bool Crouching;
    public static bool Grounded;
    public static bool LeftWall;
    public static bool RightWall;
    public float MovementSpeed;
    public float CrouchDragCoefficient;
    public float MaxJumpForce;
    public float MinJumpForce;
    public float CoyoteTime;
    private float CoyoteTimeCounter;
    public float JumpBufferTime;
    private float JumpBufferCounter;
    private float CrouchTime;
    private float CrouchTimeCounter;
    public float DashForce;
    public float DashCooldown;
    private float DashTime;
    
    private Rigidbody2D PlayerRb;
    public BoxCollider2D PlayerCollider;
    public BoxCollider2D CrouchCollider;
    public GameObject Player;
    public LayerMask TheGround;
    public Transform GroundCheck;
    public Vector2 GroundCheckRadius;
    public Transform LeftWallCheck;
    public Transform RightWallCheck;
    public Vector2 WallCheckRadius;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PlayerRb = GetComponent<Rigidbody2D>();
        Player = GameObject.Find("Player");
    }

    // Update is called once per frame
    void Update()
    {
        CheckInput();
        CheckMovementDirection();
        CheckSurroundings();
    }

    void FixedUpdate()
    {
        ApplyMovementInput();
    }

    // Checks all Overlap Colliders around the player and outputs booleans for all of them.
    private void CheckSurroundings()
    {
        Grounded = Physics2D.OverlapBox(GroundCheck.position, GroundCheckRadius, 0, TheGround);
        LeftWall = Physics2D.OverlapBox(LeftWallCheck.position, WallCheckRadius, 0, TheGround);
        RightWall = Physics2D.OverlapBox(RightWallCheck.position, WallCheckRadius, 0, TheGround);
    }

    // Checks if the player is changing direction and flips the sprite if they are.
    private void CheckMovementDirection()
    {
        if (FacingRight && MovementInputDirection < 0)
        {
            Flip();
        }
        else if (!FacingRight && MovementInputDirection > 0)
        {
            Flip();
        }
    }
    
    private void CheckInput()
    {
        // Gets the horizontal input from the player for use in movement and checking if the sprite should be flipped
        MovementInputDirection = Input.GetAxisRaw("Horizontal");
        
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

        if (Input.GetKeyDown(KeyCode.LeftShift) && DashTime <= 0)
        {
            DashTime = DashCooldown;
            Dash();
        }

        else
        {
            DashTime -= Time.deltaTime;
        }
    }
    
    // Applies the movement input to the player's rigidbody using the MovementSpeed and MovementInputDirection.
    private void ApplyMovementInput()
    {
        // If the player is walking speed remains constant, if the player is crouching speed is reduced by the CrouchDragCoefficient every frame.
        if (!Crouching) PlayerRb.linearVelocity = new Vector2(MovementSpeed * MovementInputDirection, PlayerRb.linearVelocity.y);
        else if (Crouching)
            PlayerRb.linearVelocity = Mathf.Abs(PlayerRb.linearVelocityX) > 0.2
                ? new Vector2(PlayerRb.linearVelocityX * CrouchDragCoefficient, PlayerRb.linearVelocity.y)
                : new Vector2(0, PlayerRb.linearVelocity.y);
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

    private void Dash()
    {
        for (int i = 0; i < 20; i++)
        {
            if (FacingRight && !RightWall) Player.transform.position += new Vector3(DashForce / 20f, 0, 0);
            else if (!FacingRight && !LeftWall) Player.transform.position += new Vector3(-DashForce / 20f, 0, 0);
        }
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
