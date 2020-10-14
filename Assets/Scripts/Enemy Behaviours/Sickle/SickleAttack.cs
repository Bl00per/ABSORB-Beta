using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SickleAttack : AIBehaviour
{
    [Header("Overall Properties")]
    public string swingAnimationName = "Attacking";

    [Header("Special Properties")]
    public float specialMovementSpeed = 10.0f;
    public float specialStartAnimationDistance = 5.0f;

    [Header("Elite Properties")]
    public float eliteMovementSpeed = 10.0f;            // Speed when preparing to dash
    public float eliteDashMovementSpeed = 40.0f;        // Speed of the dash
    public float eliteDashDistance = 10.0f;             // Distance the enemy will move past the player
    public float eliteStartAnimationDistance = 5.0f;    // Distance which the enemy will start the attack animation

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
        switch (enemyHandler.GetEnemyType())
        {
            case EnemyHandler.EnemyType.SPECIAL:
                // Setting the movement speed
                brain.GetNavMeshAgent().speed = specialMovementSpeed;
                break;

            case EnemyHandler.EnemyType.ELITE:
                brain.GetNavMeshAgent().speed = eliteMovementSpeed;
                break;
        }
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
            if (distance <= specialStartAnimationDistance)
                _animator.SetBool(swingAnimationName, true);
        }
    }

    private void UpdateElite()
    {
        // The elite attack is going to dash across the player and attack them when they get close enough,
        // So, step by step how we should implement it will be something like this

        // 3) When the elite is close enough to the player, they will enter there attack animation and look at the player

        // 4) After attacking, the enemy will enter back into there movement script and continuing avoiding until' there attack cooldown has expired
    }
}
