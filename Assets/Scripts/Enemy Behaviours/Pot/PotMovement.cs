using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotMovement : AIBehaviour
{
    [Header("Properties")]
    public float enterAttackStateDistance = 10.0f;
    public float retreatFromPlayerDistance = 7.0f;

    [Header("Timers")]
    public float retreatFromPlayerTimer = 0.0f;
    public float returnToInitialAngularSpeed = 1.0f;
    private float _initialAngularSpeed = 0.0f;
    private bool _startedRetreat = false;

    private void Start()
    {
        // Storing the initial angular speed of the agent
        _initialAngularSpeed = brain.GetNavMeshAgent().angularSpeed;
    }

    public override void OnStateEnter()
    {
        brain.GetNavMeshAgent().angularSpeed = _initialAngularSpeed;
    }

    public override void OnStateUpdate()
    {
        // Storing the distance to preform multiple checks on
        float distance = brain.GetDistanceToPlayer();

        //Debug.Log(GetRetreatDistance());

        // Checking if we should be locked onto the player or not...
        if (this.destinationLockedToPlayer)
            this.currentDestination = brain.PlayerTransform.position;

        // Updating the target destination every frame
        brain.SetDestinationOnCooldown(this.currentDestination, 1.0f);

        // if (distance <= retreatFromPlayerDistance)
        // {
        //     Vector3 retreatPosition = transform.position - (brain.GetDirectionToPlayer() * enterAttackStateDistance);
        //     OverrideDestination(retreatPosition, 1.0f);
        //     return;
        // }
        /* else */
        if (enemyHandler.GetJustAttacked() || distance <= retreatFromPlayerDistance)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(brain.GetDirectionToPlayer()), 0.20F);
            if (!_startedRetreat)
            {
                Vector3 retreatPosition = transform.position - (-brain.GetDirectionToPlayer() * GetRetreatDistance());
                StartCoroutine(Retreat(retreatPosition, retreatFromPlayerTimer, returnToInitialAngularSpeed));
            }
        }
        else if (distance <= enterAttackStateDistance && !_startedRetreat)
        {
            brain.SetBehaviour("Attack");
        }
        // // If the remaining distance is less than or equal to the stopping distance; enter the attack behaviour.
        // if (distance <= enterAttackStateDistance && !enemyHandler.GetJustAttacked())
        // {
        //     _attackPosition = transform.position;
        //     brain.SetBehaviour("Attack");
        // }


        // Debug.Log("Locked onto player: " + IsLockedOntoPlayer());
        // Debug.Log("Current dest: " + this.currentDestination);

    }

    public override void OnStateExit() { }

    public override void OnStateFixedUpdate() { }

    private IEnumerator Retreat(Vector3 position, float returnToPositionTimer, float returnToInitialAngularSpeedTimer)
    {
        _startedRetreat = true;
        brain.GetNavMeshAgent().angularSpeed = 0.0f;
        yield return new WaitForSeconds(returnToPositionTimer);
        OverrideDestination(position, 1.0f); ;
        yield return new WaitForSeconds(returnToInitialAngularSpeedTimer);
        brain.GetNavMeshAgent().angularSpeed = _initialAngularSpeed;
        _startedRetreat = false;
    }

    public float GetRetreatDistance()
    {
        return brain.GetDistanceToPlayer() - enterAttackStateDistance;
    }

    // [Header("Properties")]
    // public float destinationPadding = 1.0f;

    // // The inital speed; set from within the nav mesh component
    // private float _initialSpeed = 0.0f;

    // // The attack transition range; set from "stopping distance" within the nav mesh component
    // private float _attackRange = 0.0f;

    // // Called before first frame
    // private void Start()
    // {
    //     // Getting the initial speed from the nav mesh component
    //     _initialSpeed = brain.GetNavMeshAgent().speed;

    //     // Getting the attack range from the nav mesh component
    //     _attackRange = brain.GetNavMeshAgent().stoppingDistance;
    // }


    // public override void OnStateEnter()
    // {
    //     // Currently setting the on enter destination to the player; in the future we'll have to set the destination from a "EnemyAI Controller"
    //     if(this.enemyHandler.GetEnemyGroupHandler() == null)
    //         this.LockDestinationToPlayer(destinationPadding);
    // }

    // public override void OnStateUpdate()
    // {
    //     // Checking if we should be locked onto the player or not...
    //     if (this.destinationLockedToPlayer)
    //         this.currentDestination = brain.PlayerTransform.position;

    //     // Updating the target destination every frame
    //     brain.SetDestinationOnCooldown(this.currentDestination, destinationPadding);

    //     // If player is within attack range;
    //     if (brain.GetNavMeshAgent().remainingDistance <= _attackRange + 0.1F)
    //     {
    //         // Enemy will enter attack phase if locked onto player:
    //         if (this.destinationLockedToPlayer)
    //         {
    //             brain.SetBehaviour("Attack");
    //             return;
    //         }
    //         else
    //         {
    //             // Here is what they'll do when they aren't locked on
    //             // so general movement, stuff will go here when
    //             // the group system has been worked out
    //         }

    //     }
    // }

    // public override void OnStateFixedUpdate() { }

    // public override void OnStateExit() { }
}
