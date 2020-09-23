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
        weaponToEnable.SetActive(true);
        _animator.SetBool("Attacking", true);
        //_hasAttacked = false;
    }

    public override void OnStateExit()
    {
        weaponToEnable.SetActive(false);
        _animator.SetBool("Attacking", false);
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

        // if (weaponTrigger.Enabled && weaponTrigger.Collider != null && _canAttack)
        // {
        //     if (weaponTrigger.Collider.gameObject.CompareTag("Player") && !_hasAttacked)
        //     {
        //         _hasAttacked = true;
        //         _canAttack = false;
        //     }
        // }
    }

    // Activates the collision check on the enemy's weapon
    public void Key_ActivateMinionAttackCheck()
    {
        //_canAttack = true;
    }

    // Deactivates the collision check on the enemy's weapon
    public void Key_DeactivateMinionAttackCheck()
    {
        //_canAttack = false;
        brain.SetBehaviour("Movement");
    }
}
