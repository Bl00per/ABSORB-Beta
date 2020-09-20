using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EliteMovement : AIBehaviour
{

    [Header("Properties")]
    public float destinationPadding = 1.0f;

    // The inital speed; set from within the nav mesh component
    private float _initialSpeed = 0.0f;

    // The attack transition range; set from "stopping distance" within the nav mesh component
    private float _attackRange = 0.0f;

    // Called before first frame
    private void Start()
    {
        // Getting the initial speed from the nav mesh component
        _initialSpeed = brain.GetNavMeshAgent().speed;

        // Getting the attack range from the nav mesh component
        _attackRange = brain.GetNavMeshAgent().stoppingDistance;

        // Currently setting the on enter destination to the player; in the future we'll have to set the destination from a "EnemyAI Controller"
        this.LockDestinationToPlayer(destinationPadding);
    }

    public override void OnStateEnter() {}

    public override void OnStateUpdate()
    {
        // Checking if we should be locked onto the player or not...
        if (this.destinationLockedToPlayer)
            this.currentDestination = brain.PlayerTransform.position;

        // Updating the target destination every frame
        brain.SetDestinationOnCooldown(this.currentDestination, destinationPadding);

        // If player is within attack range;
        if(brain.GetNavMeshAgent().remainingDistance <= _attackRange)
        {
            // Enemy will enter attack phase if locked onto player:
            if(this.destinationLockedToPlayer)
            {
                brain.SetBehaviour("Attack");
                return;
            }
            else
            {
                // Here is what they'll do when they aren't locked on
                // so general movement, stuff will go here when
                // the group system has been worked out
            }
            
        }
    }

    public override void OnStateFixedUpdate() {}

    public override void OnStateExit(){}
    
    // [Header("General Movement")]
    // public float acceleration = 50.0f;
    // public float retreatDashAcceleration = 75.0f;
    // public float retreatMaxVelocity = 20.0f;
    // public float maxVelocity = 10.0f;
    // public float turnSpeed = 0.20f;

    // [Header("Attack")]
    // public float attackDistance = 15.0f;
    // public float retreatDistance = 15.0f;
    // public float beforeAttackTimer = 0.7f;
    // private bool _isWaitingToAttack = false;

    // public override void OnStateEnter() 
    // {
    //     _isWaitingToAttack = false;
    // }

    // public override void OnStateExit() {}

    // public override void OnStateFixedUpdate()
    // {
    //     // If the enemy is already waiting to attack, then exit this out of this function
    //     if (_isWaitingToAttack)
    //         return;

    //     // Get direction and distance from player
    //     Vector3 dir = brain.GetDirectionToPlayer();
    //     float dist = brain.GetDistanceToPlayer();

    //     // Rotate to face direction
    //     transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), turnSpeed);

    //     // Retreat from player if they get too close
    //     if (dist < attackDistance)
    //     {
    //         // Only adding force if velocity is under max velocity
    //         if (rigidbody.velocity.magnitude < retreatMaxVelocity)
    //         {
    //             // Moving towards player if out of attack distance
    //             rigidbody.AddForce(-transform.forward * retreatDashAcceleration * Time.fixedDeltaTime, ForceMode.Impulse);
    //         }
    //     }

    //     // Only adding force if velocity is under max velocity
    //     if (rigidbody.velocity.magnitude < maxVelocity && dist > attackDistance)
    //     {
    //         // Moving towards player if out of attack distance
    //         rigidbody.AddForce(transform.forward * acceleration * Time.fixedDeltaTime, ForceMode.Impulse);
    //     }

    //     // If the enemy is at the optimal attack range, enter the attack state
    //     if (Mathf.Ceil(dist) == attackDistance)
    //         StartCoroutine(AttackSequence());
    // }

    // public override void OnStateUpdate() {}

    // public IEnumerator AttackSequence()
    // {
    //     _isWaitingToAttack = true;
    //     yield return new WaitForSecondsRealtime(beforeAttackTimer);
    //     brain.SetBehaviour("Attack");
    //     _isWaitingToAttack = false;
    // }
}
