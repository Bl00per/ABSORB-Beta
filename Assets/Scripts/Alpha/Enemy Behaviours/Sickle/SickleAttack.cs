using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SickleAttack : AIBehaviour
{
    [Header("Properties")]
    public string swingAnimationName = "Attacking";
    public int amountOfAttacks = 4;
    public float speedMultiplier = 1.5f;
    public float cancelAttackDistance = 3.0f;

    private int _attackIndex = 0;
    private float _defaultSpeed = 1.0f;

    private Animator _animator;
    private float _initialSpeed = 20.0f;

    private void Awake()
    {
        _animator = this.GetComponent<Animator>();
        _defaultSpeed = _animator.GetFloat("attackSpeed");
    }

    private void Start()
    {
        _initialSpeed = brain.GetNavMeshAgent().speed;
    }

    public override void OnStateEnter()
    {
        // Set the attacking bool to true
        _animator.SetBool(swingAnimationName, true);
    }

    public override void OnStateUpdate() {}

    public override void OnStateFixedUpdate() {}

    public override void OnStateExit()
    {
        brain.GetNavMeshAgent().speed = _initialSpeed;
    }

    private void Key_DeactivateSwingAnimation()
    {
        if(_attackIndex < amountOfAttacks && brain.GetDistanceToPlayer() <= cancelAttackDistance)
        {
            _animator.SetBool(swingAnimationName, true);
            _animator.SetFloat("attackSpeed", _animator.GetFloat("attackSpeed") * speedMultiplier);
            _attackIndex++;
        }
        else
        {
            _animator.SetBool(swingAnimationName, false);
            _animator.SetFloat("attackSpeed", _defaultSpeed);
            brain.GetNavMeshAgent().isStopped = false;
            brain.SetBehaviour("Movement");
            _attackIndex = 0;
        }
    }
}
