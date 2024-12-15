using UnityEngine;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{

    private float MovementInputDirection;
    private float time;
    private bool FacingRight = true;
    public bool Grounded;
    public float MovementSpeed;
    public float JumpForce;
    
    private Rigidbody2D PlayerRb;
    public Animator PlayerAnim;
    public GameObject Player;
    public LayerMask TheGround;
    public Transform GroundCheck;
    public Vector2 GroundCheckRadius;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PlayerRb = GetComponent<Rigidbody2D>();
        PlayerAnim = GetComponent<Animator>();
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
        PlayerAnim.SetFloat("MoveSpeed", Mathf.Abs(MovementInputDirection));
        if (MovementInputDirection == 0)
        {
            time += Time.deltaTime;
            if (time >= 0.1f)
            {
                PlayerAnim.SetTrigger("Idle");
                time = 0;
            }
        }
        if (Input.GetKeyDown(KeyCode.Space) && Grounded)
        {
            Jump();
        }

        if (Grounded)
        {
            PlayerAnim.SetBool("Grounded", true);
        }
        else
        {
            PlayerAnim.SetBool("Grounded", false);
        }
    }
    
    private void ApplyMovementInput()
    {
        PlayerRb.linearVelocity = new Vector2(MovementSpeed * MovementInputDirection, PlayerRb.linearVelocity.y);
    }

    private void Jump()
    {
        PlayerRb.linearVelocity = new Vector2(PlayerRb.linearVelocity.x, JumpForce);
        PlayerAnim.SetTrigger("Jump");
    }

    private void Flip()
    {
        FacingRight = !FacingRight;
        transform.Rotate(0,180,0);
    }
}
