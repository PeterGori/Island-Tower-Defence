using UnityEngine;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{

    public static float MovementInputDirection;
    private float time;
    private bool FacingRight = true;
    public static bool Grounded;
    public float MovementSpeed;
    public float MaxJumpForce;
    public float MinJumpForce;
    public float CoyoteTime;
    private float CoyoteTimeCounter;
    public float JumpBufferTime;
    private float JumpBufferCounter;
    
    private Rigidbody2D PlayerRb;
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

    private void CheckSurroundings()
    {
        Grounded = Physics2D.OverlapBox(GroundCheck.position, GroundCheckRadius, 0, TheGround);
    }

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
        MovementInputDirection = Input.GetAxisRaw("Horizontal");
        if (Grounded)
        {
            CoyoteTimeCounter = CoyoteTime;
        }
        else
        {
            CoyoteTimeCounter -= Time.deltaTime;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            JumpBufferCounter = JumpBufferTime;
        }
        else
        {
            JumpBufferCounter -= Time.deltaTime;
        }

        if (Input.GetKeyUp(KeyCode.Space) && PlayerRb.linearVelocity.y > MinJumpForce)
        {
            PlayerRb.linearVelocity = new Vector2(PlayerRb.linearVelocity.x, MinJumpForce);
        }
        
        if(JumpBufferCounter > 0 && CoyoteTimeCounter > 0f)
        {
            Jump();
            JumpBufferCounter = 0;
        }
    }
    
    private void ApplyMovementInput()
    {
        PlayerRb.linearVelocity = new Vector2(MovementSpeed * MovementInputDirection, PlayerRb.linearVelocity.y);
    }

    private void Jump()
    {
        PlayerRb.linearVelocity = new Vector2(PlayerRb.linearVelocity.x, MaxJumpForce);
        PlayerAnimator.Jump();
    }

    private void Flip()
    {
        FacingRight = !FacingRight;
        transform.Rotate(0,180,0);
    }
}
