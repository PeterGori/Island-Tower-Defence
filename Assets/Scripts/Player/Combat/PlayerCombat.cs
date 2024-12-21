using System.Collections;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private float ComboTime;
    [SerializeField] private float ComboWait;
    private float ComboTimeCounter = 0;
    private float CoolDownCounter = 0;
    private bool Combo = false;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        CheckInput();
        ComboTimeCounter -= Time.deltaTime;
        CoolDownCounter -= Time.deltaTime;
        // print("ComboTimeCounter: " + ComboTimeCounter);
    }
    
    private void CheckInput()
    {
        if (Input.GetKeyDown(KeyCode.K) && ComboTimeCounter is > 0 and < 0.45f)
        {
            Combo = true;
            ComboTimeCounter = 0;
            PlayerAnimator.Combo();
        }
        
        else if (Input.GetKeyDown(KeyCode.K) && CoolDownCounter <= 0)
        {
            Attack();
            ComboTimeCounter = ComboTime;
            CoolDownCounter = ComboTime;
            PlayerAnimator.Attack();
        }
        
        if (ComboTimeCounter <= -ComboWait && Combo)
        {
            PlayerAnimator.EndCombo();
            Combo = false;
            Attack();
        }
    }
    
    private void Attack()
    {
        print("Attack");
    }
}
