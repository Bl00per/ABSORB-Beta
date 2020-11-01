using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MinionMovement : AIBehaviour
{
    [Header("Properties")]
    public float enterAttackStateDistance = 10.0f;
    public float retreatDistance = 20.0f;

    [Header("Timers")]
    public float returnToAttackPosition = 1.0f;
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

        // // Checking if we should be locked onto the player or not...
        // if (this.destinationLockedToPlayer)
        //     this.currentDestination = brain.PlayerTransform.position;

        // // Updating the target destination every frame
        // brain.SetDestinationOnCooldown(this.currentDestination, 1.0f);

        if (enemyHandler.GetJustAttacked())
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(brain.GetDirectionToPlayer()), 0.20F);
            
            if (!_startedRetreat)
            {
                Vector3 retreatPosition = transform.position - (brain.GetDirectionToPlayer() * retreatDistance);
                StartCoroutine(Retreat(retreatPosition));
            }
        }
        else if (distance <= enterAttackStateDistance)
        {
            brain.SetBehaviour("Attack");
        }
    }

    public override void OnStateExit() { }

    public override void OnStateFixedUpdate() { }

    private IEnumerator Retreat(Vector3 position)
    {
        _startedRetreat = true;
        brain.GetNavMeshAgent().angularSpeed = 0.0f;
        yield return new WaitForSeconds(returnToAttackPosition);
        OverrideDestination(position, 1.0f);
        yield return new WaitForSeconds(returnToInitialAngularSpeed);
        brain.GetNavMeshAgent().angularSpeed = _initialAngularSpeed;
        _startedRetreat = false;
    }
    /*

        Creating a movement state machine to help control each state better;
            - SPRINT: When far away, the minion will move at a faster 
                      acceleration towards the player.
            - NORMAL: Once close to the player, they will move at their normal speed
            - AVOID: Once in attack range, the minion has a chance to either enter the
                     attack phase or avoid the player.

        Normal is the main state, which will be set on enter.

        Would be cool to create a "buddy" system, where the minion would retreat if 
        they are alone.

        If not, creating a pool like system that will fill with surrounding enemies,
        that can control the movement of the pack would be useful.
        Eg; Create 3 states which the pool can be in
            - Elite Filled; There are more elites than any other enemy in this pool; 
            - Special Filled; There are more specials than any other enemy in this pool; 
            - Minion Filled; There are more minions than any other enemy in this pool; 

        This means I would have to create functions within the AI brain to talk to the movement
        and attack behaviours, to direct the enemies.

        Creating an array of AIBrains, then using a flocking like function to control the groups
        current distination.

        void SetTargetDestination(Vector3 dest)
        {
            // Sets target destionation and updates nav mesh
        }

    */

    // [Header("Properties")]
    // public float sprintSpeed = 20.0f;
    // public float sprintRange = 20.0f;
    // public float avoidSpeed = 10.0f;
    // [Range(0, 1)]
    // public float avoidChance = 0.2f;
    // public float avoidRadius = 15.0f;
    // public float destinationPadding = 0.5f;

    // // The inital speed; set from within the nav mesh component
    // private float _initialSpeed = 0.0f;

    // // The attack transition range; set from "stopping distance" within the nav mesh component
    // private float _attackRange = 0.0f;

    // // A flag to determine if the enemy is avoiding or not
    // private bool _isAvoiding = false;

    // // Sub FSM to control the movement states
    // private enum MovementState
    // {
    //     NORMAL,
    //     SPRINTING,
    //     COMBAT_TRANSITION,
    // }
    // private MovementState _currentState;

    // // The distance to the player; updated every frame
    // private float _distanceToDestination = 0.0f;

    // // Called on initialise
    // private void Awake() { }

    // // Called before first frame
    // private void Start()
    // {
    //     // Getting the initial speed from the nav mesh component
    //     _initialSpeed = brain.GetNavMeshAgent().speed;

    //     // Getting the attack range from the nav mesh component
    //     _attackRange = brain.GetNavMeshAgent().stoppingDistance;
    // }

    // // Called on enter
    // public override void OnStateEnter()
    // {
    //     // Setting the entry state to "normal"
    //     _currentState = MovementState.NORMAL;

    //     // Currently setting the on enter destination to the player; in the future we'll have to set the destination from a "EnemyAI Controller"
    //     //this.LockDestinationToPlayer(destinationPadding);
    // }

    // // Called every frame
    // public override void OnStateUpdate()
    // {
    //     // Updating all FSM logic
    //     UpdateLogic();

    //     // Update the FSM
    //     switch (_currentState)
    //     {
    //         case MovementState.NORMAL:
    //             UpdateNormalState();
    //             break;

    //         case MovementState.SPRINTING:
    //             UpdateSprintingState();
    //             break;

    //         case MovementState.COMBAT_TRANSITION:
    //             UpdateCombatTransition();
    //             break;
    //     }
    // }

    // // Called every physics
    // public override void OnStateFixedUpdate() { }

    // // Called on exit
    // public override void OnStateExit() { }

    // // Checks the current distance from the player and determines what state to be in
    // private void UpdateLogic()
    // {
    //     // Updating the distance to the final destination
    //     _distanceToDestination = brain.GetNavMeshAgent().remainingDistance;

    //     // Checking if we should be locked onto the player or not...
    //     if (this.destinationLockedToPlayer)
    //         this.currentDestination = brain.PlayerTransform.position;

    //     // If player is out of sprint range;
    //     if (_distanceToDestination > sprintRange && _currentState == MovementState.NORMAL)
    //     {
    //         // Updating the current speed to sprint speed;
    //         brain.GetNavMeshAgent().speed = sprintSpeed;

    //         // Setting the current state to sprinting
    //         _currentState = MovementState.SPRINTING;
    //     }

    //     // If player is in sprint range;
    //     else if (_distanceToDestination < sprintRange && _currentState == MovementState.SPRINTING)
    //     {
    //         // Updating the current speed to the inital speed
    //         brain.GetNavMeshAgent().speed = _initialSpeed;

    //         // Setting the current state to normal
    //         _currentState = MovementState.NORMAL;
    //     }

    //     // If player is close enough to attack;
    //     else if (_distanceToDestination <= _attackRange)
    //     {
    //         // If locked onto player, enter the combat transition
    //         if (destinationLockedToPlayer)
    //         {
    //             // Updating the current speed to the inital speed
    //             brain.GetNavMeshAgent().speed = _initialSpeed;

    //             // Setting the current state to the combat transtion
    //             _currentState = MovementState.COMBAT_TRANSITION;
    //         }
    //         // If not locked on but close to final destination
    //         else
    //         {
    //             // Currently just locking back onto the player after avoiding;
    //             if (_isAvoiding)
    //             {
    //                 //LockDestinationToPlayer(destinationPadding);
    //                 _isAvoiding = false;
    //             }
    //             else
    //             {
    //                 // This should be called when the enemy reachs the end position, when they arent targeting the player
    //             }
    //         }
    //     }
    // }

    // // The "Normal" state update
    // private void UpdateNormalState()
    // {
    //     brain.SetDestinationOnCooldown(this.currentDestination, destinationPadding);
    // }

    // // The "Sprinting" state update
    // private void UpdateSprintingState()
    // {
    //     brain.SetDestinationOnCooldown(this.currentDestination, destinationPadding);
    // }

    // // The "Avoid" state update
    // private void UpdateCombatTransition()
    // {
    //     if(enemyHandler.GetJustAttacked())
    //         return;

    //     // Check if the enemy should avoid or attack
    //     // For now, just finding a random position around the player and moving to there
    //     // Once I have implemented groups, the enemy will avoid according to the groups position.       else
    //     if (Random.value <= avoidChance)
    //         ForceAvoidState();
    //     {
    //         // If attack
    //         brain.SetBehaviour("Attack");
    //         ForceAvoidState();
    //     }
    // }

    // // Forces the avoid function
    // // TODO: Make this check if the destination is on the same Y or something
    // // because the minion can run all the way down a platform when trying to avoid
    // public void ForceAvoidState()
    // {
    //     _isAvoiding = true;
    //     this.OverrideDestination(GetRandomizedPositionAroundCenter(brain.PlayerTransform.position, avoidRadius), 1.0f);
    // }

    // // Returns a randomized position from the radius around the center of an object
    // // This function will be replaced when "Unit Slotting" or "AI Group Control" gets implemented.
    // private Vector3 GetRandomizedPositionAroundCenter(Vector3 center, float radius)
    // {
    //     // create random angle between 0 to 360 degrees 
    //     float ang = Random.value * 360;
    //     Vector3 pos = Vector3.zero;
    //     pos.x = center.x + radius * Mathf.Sin(ang * Mathf.Deg2Rad);
    //     pos.y = center.y + radius * Mathf.Cos(ang * Mathf.Deg2Rad);
    //     pos.z = center.z;
    //     return pos;
    // }

    /*
    [Header("General Movement")]
    public float acceleration = 50.0f;
    public float maxVelocity = 10.0f;
    public float turnSpeed = 0.20f;
    private NavMeshAgent _navMeshAgent;

    [Header("Sprinting")]
    public float sprintAcceleration = 75.0f;
    public float sprintMaxVelocity = 15.0f;
    public float rangeOfSprint = 7.5f;

    [Header("Dodge")]
    [Range(0.0f, 1.0f)]
    public float dodgeChance = 1.0f;
    public float dodgeAcceleration = 5.0f;
    public float dodgeMaxVelocity = 15.0f;
    public float dodgeDistance = 10.0f;

    [Header("Attack Transition")]
    public float attackRange = 2.0f;
    private bool _shouldDodge = false;
    */

    // public override void OnFixedUpdate() 
    // {
    //     // Get distance and direction towards player
    //     float dist = brain.GetDistanceToPlayer();
    //     Vector3 dir = brain.GetDirectionToPlayer();

    //     // Rotate to face direction
    //     transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), turnSpeed);

    //     // Check if close enough to attack
    //     if(dist <= attackRange && !_shouldDodge)
    //     {
    //         // Check if we should attack, or avoid.
    //         if(Random.value <= dodgeChance)
    //             _shouldDodge = true;
    //         else
    //         {
    //             brain.SetBehaviour("Attack");
    //             _shouldDodge = true;
    //         }
    //     }

    //     // Checking if the minion should dodge
    //     if(_shouldDodge)
    //     {
    //         // Move towards player faster if out of sprint range
    //         if (dist < dodgeDistance && rigidbody.velocity.magnitude < dodgeMaxVelocity)
    //         {
    //             rigidbody.AddForce(-transform.forward * dodgeAcceleration * Time.fixedDeltaTime, ForceMode.Impulse);
    //         }

    //         // Check if we should stop dodging
    //         if(dist >= dodgeDistance)
    //             _shouldDodge = false;

    //         return;
    //     }

    //     // Move towards player faster if out of sprint range
    //     if(dist > rangeOfSprint && rigidbody.velocity.magnitude < sprintMaxVelocity)
    //     {
    //         rigidbody.AddForce(transform.forward * sprintAcceleration * Time.fixedDeltaTime, ForceMode.Impulse);
    //     }

    //     // Move towards player slower if within sprint range
    //     else if(rigidbody.velocity.magnitude < maxVelocity)
    //     {
    //         rigidbody.AddForce(transform.forward * acceleration * Time.fixedDeltaTime, ForceMode.Impulse);
    //     }
    // }
}
