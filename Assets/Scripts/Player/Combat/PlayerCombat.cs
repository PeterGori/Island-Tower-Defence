using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        CheckInput();
    }
    
    private void CheckInput()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            Attack();
            PlayerAnimator.Attack();
        }
    }
    
    private void Attack()
    {
        print("Attack");
    }
}
