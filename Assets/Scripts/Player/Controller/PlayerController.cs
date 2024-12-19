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
    public float MovementSpeed;
    public float WalkSpeed;
    public float CrouchSpeed;
    public float MaxJumpForce;
    public float MinJumpForce;
    public float CoyoteTime;
    private float CoyoteTimeCounter;
    public float JumpBufferTime;
    private float JumpBufferCounter;
    private float CrouchTime;
    private float CrouchTimeCounter;
    
    private Rigidbody2D PlayerRb;
    public BoxCollider2D PlayerCollider;
    public BoxCollider2D CrouchCollider;
    public GameObject Player;
    public LayerMask TheGround;
    public Transform GroundCheck;
    public Vector2 GroundCheckRadius;
    
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
            MovementSpeed = CrouchSpeed;
            // CrouchTimeCounter = CrouchTime;
        }
        // If the player is not pressing the S key, the player stands up and the movement speed is reset to the walk speed.
        else
        {
            MovementSpeed = WalkSpeed;
            PlayerCollider.enabled = true;
            CrouchCollider.enabled = false;
            Crouching = false;
        }
    }
    
    // Applies the movement input to the player's rigidbody using the MovementSpeed and MovementInputDirection.
    private void ApplyMovementInput()
    {
        PlayerRb.linearVelocity = new Vector2(MovementSpeed * MovementInputDirection, PlayerRb.linearVelocity.y);
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

    // Flips the player sprite when the player changes direction.
    private void Flip()
    {
        FacingRight = !FacingRight;
        transform.Rotate(0,180,0);
    }
    
}
