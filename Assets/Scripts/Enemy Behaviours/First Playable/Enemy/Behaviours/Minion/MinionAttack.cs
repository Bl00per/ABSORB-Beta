using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionAttack : AIBehaviour
{
    [Header("General Movement")]
    public bool lookAtPlayer = true;
    public float turnSpeed = 0.20f;

    [Header("References")]
    public Trigger weaponTrigger;
    public GameObject weaponToEnable;
    private Animator _animator;
    private CombatHandler _combatHandler;

    [Header("Timers")]
    public float transitionTime = 2.0f;
    public float timeBeforeAttack = 0.5f;

    [Header("Attack Properties")]
    public float attackForce = 10.0f;

    // private bool _hasAttacked = false;
    // private bool _canAttack = false;

    private void Awake()
    {
        _animator = this.GetComponent<Animator>();
    }

    private void Start()
    {
        _combatHandler = this.brain.PlayerTransform.GetComponent<CombatHandler>();
    }

    public override void OnStateEnter()
    {
        
        _animator.SetBool("Attacking", true);
        enemyHandler.SetJustAttacked(true);
        //_hasAttacked = false;
    }

    public override void OnStateExit()
    {
        
        _animator.SetBool("Attacking", false);
        StartCoroutine(enemyHandler.Coroutine_JustAttacked());
       // _hasAttacked = false;
        //_canAttack = false;
    }

    public override void OnStateFixedUpdate() {}

    public override void OnStateUpdate() 
    {
        // Get direction to player
        Vector3 dir = brain.GetDirectionToPlayer();

        // Rotate to face direction
        if(lookAtPlayer)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), turnSpeed);
    }
   
    public void Key_ActivateMinionAttack()
    {

        weaponToEnable.SetActive(true);
    }
    // Deactivates the collision check on the enemy's weapon
    public void Key_DeactivateMinionAttack()
    {
        weaponToEnable.SetActive(false);
        //_canAttack = false;
        brain.SetBehaviour("Movement");
    }
}
