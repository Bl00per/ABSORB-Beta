﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SickleAttack : AIBehaviour
{
    [Header("Properties")]
    public float movementSpeed = 10.0f;
    public float startAnimationDistance = 5.0f;
    public string swingAnimationName = "Attacking";

    private Animator _animator;
    private float _initialSpeed = 0.0f;

    private void Awake()
    {
        _animator = this.GetComponent<Animator>();
    }

    private void Start()
    {
        _initialSpeed = brain.GetNavMeshAgent().speed;
        _animator = enemyHandler.GetAnimator();
    }

    public override void OnStateEnter()
    {
        // Setting the movement speed
        brain.GetNavMeshAgent().speed = movementSpeed;
    }

    public override void OnStateUpdate()
    {

        switch (enemyHandler.GetEnemyType())
        {
            case EnemyHandler.EnemyType.SPECIAL:
                UpdateSpecial();
                break;

            case EnemyHandler.EnemyType.ELITE:
                UpdateElite();
                break;
        }
    }

    public override void OnStateFixedUpdate() { }

    public override void OnStateExit()
    {
        brain.GetNavMeshAgent().speed = _initialSpeed;
        _animator.SetBool(swingAnimationName, false);
    }

    // Key Event: Used to transition back into the movement state.
    private void Key_DeactivateSwingAnimation()
    {
        _animator.SetBool(swingAnimationName, false);
        brain.SetBehaviour("Movement");
    }

    // Returns true if the player is moving towards the enemy, or if they aren't moving fast enough to avoid the attack.
    private bool DetermineAttackFromPlayerVelocity()
    {
        Rigidbody rb = enemyHandler.GetPlayerHandler().GetRigidbody();
        if (rb.velocity.magnitude <= 3.0F)
            return true;
        else
        {
            Vector3 rbDir = rb.velocity.normalized;
            float dot = Vector3.Dot(transform.forward, rbDir);
            return Vector3.Dot(transform.forward, rbDir) < 0.0F;
        }
    }

    private void UpdateSpecial()
    {
        // Setting the target destination
        this.LockDestinationToPlayer(1.0f);

        // Checking if the player is close enough to start the animation sequence
        if (DetermineAttackFromPlayerVelocity())
        {
            float distance = brain.GetDistanceToPlayer();
            if (distance <= startAnimationDistance)
                _animator.SetBool(swingAnimationName, true);
        }
    }

    private void UpdateElite()
    {
    }
    // [Header("Properties")]
    // public string swingAnimationName = "Attacking";
    // public int amountOfAttacks = 4;
    // public float speedMultiplier = 1.5f;
    // public float cancelAttackDistance = 3.0f;

    // private int _attackIndex = 0;
    // private float _defaultSpeed = 1.0f;

    // private Animator _animator;
    // private float _initialSpeed = 20.0f;

    // private void Awake()
    // {
    //     _animator = this.GetComponent<Animator>();
    //     _defaultSpeed = _animator.GetFloat("attackSpeed");
    // }

    // private void Start()
    // {
    //     _initialSpeed = brain.GetNavMeshAgent().speed;
    // }

    // public override void OnStateEnter()
    // {
    //     // Set the attacking bool to true
    //     _animator.SetBool(swingAnimationName, true);
    // }

    // public override void OnStateUpdate() {}

    // public override void OnStateFixedUpdate() {}

    // public override void OnStateExit()
    // {
    //     brain.GetNavMeshAgent().speed = _initialSpeed;
    // }

    // private void Key_DeactivateSwingAnimation()
    // {
    //     if(_attackIndex < amountOfAttacks && brain.GetDistanceToPlayer() <= cancelAttackDistance)
    //     {
    //         _animator.SetBool(swingAnimationName, true);
    //         _animator.SetFloat("attackSpeed", _animator.GetFloat("attackSpeed") * speedMultiplier);
    //         _attackIndex++;
    //     }
    //     else
    //     {
    //         _animator.SetBool(swingAnimationName, false);
    //         _animator.SetFloat("attackSpeed", _defaultSpeed);
    //         brain.GetNavMeshAgent().isStopped = false;
    //         brain.SetBehaviour("Movement");
    //         _attackIndex = 0;
    //     }
    // }
}
