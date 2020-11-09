using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
    public float eliteStartDashDistance = 10.0f;        // Distance which the enemy will start dashing towards the player
    public float eliteDashDistance = 10.0f;             // Distance the enemy will move past the player
    public float eliteStartAnimationDistance = 5.0f;    // Distance which the enemy will start the attack animation
    public float eliteDashOffsetMultiplier = 1.5f;      // Determines how far the elite will move left or right when dashing towards the player
    private bool _isDashing = false;

    private Animator _animator;
    private float _initialSpeed = 0.0f;
    private float _initialAngularSpeed = 0.0f;
    private Vector3 _positionFix = Vector3.zero;
    private Vector3 _dashDestination = Vector3.zero;
    private bool _justAttacked = false;

    private void Awake()
    {
        _animator = this.GetComponent<Animator>();
    }

    private void Start()
    {
        _initialAngularSpeed = brain.GetNavMeshAgent().angularSpeed;
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
                _isDashing = false;
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
        brain.GetNavMeshAgent().angularSpeed = _initialAngularSpeed;
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
        this.LockDestinationToPlayer();

        // Checking if the player is close enough to start the animation sequence
        if (DetermineAttackFromPlayerVelocity())
        {
            float distance = brain.GetDistanceToPlayer();
            if (distance <= specialStartAnimationDistance)
                _animator.SetBool(swingAnimationName, true);
        }
    }

    private Vector3 GetRandomDirection()
    {
        if (Random.Range(0, 1) == 0)
            return brain.transform.right;
        else
            return -brain.transform.right;
    }

    private void UpdateElite()
    {
        float distance = brain.GetDistanceToPlayer();

        // Look at player if moving towards
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(brain.GetDirectionToPlayer()), 0.20F);

        // If the enemy isn't dashing
        if (!_isDashing)
        {
            // Check if we are close enough to start dashing
            if (distance <= eliteStartDashDistance)
            {
                _dashDestination = brain.PlayerTransform.position + ((brain.GetDirectionToPlayer() * eliteDashDistance) + (GetRandomDirection() * eliteDashOffsetMultiplier));
                NavMeshPath navMeshPath = new NavMeshPath();
                if (brain.GetNavMeshAgent().CalculatePath(_dashDestination, navMeshPath) && navMeshPath.status != NavMeshPathStatus.PathComplete)
                {
                    _dashDestination = brain.PlayerTransform.position;
                }

                brain.GetNavMeshAgent().speed = eliteDashMovementSpeed;
                brain.GetNavMeshAgent().angularSpeed = 0.0f;
                OverrideDestination(_dashDestination);
                _isDashing = true;
            }
            // Move towards player if not dashing or within start dash distance
            else
            {
                if(enemyHandler.GetPlayerHandler().GetIsAlive())
                    this.LockDestinationToPlayer();
                else
                    brain.SetBehaviour("Movement");
            }
        }
        // If they are dashing
        else
        {
            if (!_justAttacked)
            {
                // Start attacking the player when in distance
                if (distance <= eliteStartAnimationDistance)
                {
                    _animator.SetBool(swingAnimationName, true);
                    _justAttacked = true;
                }
            }
            else if (brain.GetNavMeshAgent().remainingDistance <= brain.GetNavMeshAgent().stoppingDistance + 1.0f)
            {
                brain.GetNavMeshAgent().angularSpeed = _initialAngularSpeed;
                _animator.SetBool(swingAnimationName, false);
                brain.SetBehaviour("Movement");
                _justAttacked = false;
                _isDashing = false;
            }

            //Debug.Log("Meant to be under: " + brain.GetNavMeshAgent().stoppingDistance + 1.0f + ".... but is actually: " + Vector3.Distance(brain.transform.position, _dashDestination));
        }


        // The elite attack is going to dash across the player and attack them when they get close enough,
        // So, step by step how we should implement it will be something like this

        // 3) When the elite is close enough to the player, they will enter there attack animation and look at the player

        // 4) After attacking, the enemy will enter back into there movement script and continuing avoiding until' there attack cooldown has expired
    }
}
