using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialParried : AIBehaviour
{
    [Header("References")]
    public Animator animator;

    [Header("Properties")]
    public float exitTime = 3.0f;
    public bool lockState = false;
    public string stateReturnName = "Idle";

    // private AbilityManager playerAbililtyManager;
    
    private bool _isAbsorbable = false;

    private void Start()
    {
        // playerAbililtyManager = brain.PlayerTransform.GetComponent<AbilityManager>();

        //if (lockState)
            //playerAbililtyManager.SetAbsorbTarget(brain);
    }

    public override void OnStateEnter() 
    {
        _isAbsorbable = true;
        animator?.SetBool("Parried", true);
        animator?.SetBool("Attacking", false);
        if (!lockState)
            StartCoroutine(ExitSequence());
    }

    public override void OnStateExit() 
    {
        // playerAbililtyManager.LastParriedEnemy = null;
        // playerAbililtyManager.SetAbsorbTarget(null);
        animator?.SetBool("Parried", false);
        _isAbsorbable = false;
    }

    public override void OnStateFixedUpdate() {}

    public override void OnStateUpdate() 
    {
        if(!_isAbsorbable)
        {
            brain.SetBehaviour(stateReturnName);
            // playerAbililtyManager.LastParriedEnemy = null;
            // playerAbililtyManager.SetAbsorbTarget(null);
        }
    }

    private IEnumerator ExitSequence()
    {
        yield return new WaitForSecondsRealtime(exitTime);
        _isAbsorbable = false;
    }

    public bool GetAbsorbable()
    {
        return _isAbsorbable;
    }
}
