using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    public static Animator PlayerAnim;
    public GameObject Player;

    private float time = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PlayerAnim = GetComponent<Animator>();
        Player = GameObject.Find("Player");
    }

    // Update is called once per frame
    void Update()
    {
        time += UnityEngine.Time.deltaTime;
        PlayerAnim.SetFloat("MoveSpeed", Mathf.Abs(PlayerController.MovementInputDirection));
        if (PlayerController.MovementInputDirection == 0)
        {
            time += Time.deltaTime;
            if (time >= 0.1f)
            {
                PlayerAnim.SetTrigger("Idle");
                time = 0;
            }
        }
        
        if (PlayerController.Grounded)
        {
            PlayerAnim.SetBool("Grounded", true);
        }
        else
        {
            PlayerAnim.SetBool("Grounded", false);
        }
    }
    
    public static void Jump()
    {
        PlayerAnim.SetTrigger("Jump");
    }
}
