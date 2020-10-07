using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupCombat : GroupState
{
    [Header("Group Movement Properties")]
    public float returnToChaseDistance = 12.0f;
    public int minGroupWanderRadius = 5;
    public int maxGroupWanderRadius = 10;

    [Header("Unit Slotting Properties")]
    public float queueTime = 1.5f;

    private bool queueFlag = false;
    private int _activeIndex = 0;
    private Vector3 _positionFix = Vector3.zero;
    private Vector3 _attackerLastPosition = Vector3.zero;
    private List<EnemyHandler> _unitSlots = new List<EnemyHandler>();

    public override void OnStateEnter()
    {
        // Sorting unit slots by closest to player
        UpdateUnitSlots();
        _activeIndex = 0;
    }

    public override void OnStateUpdate()
    {
        // If the group is too far from the player, enter back into chase state
        if (GetFirstDistanceToPlayer() >= returnToChaseDistance)
        {
            enemyGroupHandler.SetState(EnemyGroupHandler.E_GroupState.CHASE);
            return;
        }

        // // Setting up an enemy for an attack
        // if (!queueFlag && _unitSlots.Count > 0)
        //     StartCoroutine(QueueAttack());


        // if (enemyGroupHandler.GetEnemies().Count == 1)
        // {
        //     // Get the enemy brain at this index
        //     AIBrain aiBrain = enemyGroupHandler.GetEnemy(0).GetBrain();

        //     // Forcing the enemy to face the player
        //     _positionFix = aiBrain.PlayerTransform.position;
        //     _positionFix.y = aiBrain.transform.position.y;
        //     aiBrain.transform.forward = (_positionFix - aiBrain.transform.position).normalized;

        //     // If this enemy hasn't attacked recently 
        //     if (!aiBrain.GetHandler().GetJustAttacked())
        //     {
        //         aiBrain.GetAIBehaviour("Movement").LockDestinationToPlayer(1.0f);
        //     }
        //     else
        //     {

        //     }

        //     return;
        // }

        // if (this.enemyGroupHandler.GetCOMDistanceFromPlayer() <= beginHuddleDistance)
        // {
        //     // Queue slot for attack
        //     if (!queueFlag)
        //         StartCoroutine(QueueAttack());

        //     // Iterating over the enemy list within the group handler
        //     for (int i = 0; i < this.enemyGroupHandler.GetEnemies().Count; ++i)
        //     {
        //         // Get the enemy brain at this index
        //         AIBrain aiBrain = enemyGroupHandler.GetEnemy(i).GetBrain();

        //         // Checking if the enemy isn't attacking the player
        //         if (!aiBrain.GetAIBehaviour("Movement").IsLockedOntoPlayer() && !aiBrain.GetHandler().IsParried() && _unitSlots[_activeIndex].GetFunctional())
        //         {
        //             // Forcing the enemy to face the player
        //             _positionFix = aiBrain.PlayerTransform.position;
        //             _positionFix.y = aiBrain.transform.position.y;
        //             aiBrain.transform.forward = (_positionFix - aiBrain.transform.position).normalized;

        //             if (aiBrain.GetDistanceToPlayer() >= stopHuddleDistance)
        //             {
        //                 // Move enemy into position around the player according to index
        //                 aiBrain.GetAIBehaviour("Movement").OverrideDestination(GetPositionAroundPoint(aiBrain, aiBrain.PlayerTransform.position, i), 1.0f);
        //             }
        //         }
        //     }
        // }
        // else
        // {
        //     // Returning to chase if not in distance
        //     enemyGroupHandler.SetState(EnemyGroupHandler.E_GroupState.CHASE);
        // }
    }

    public override void OnStateFixedUpdate() { }

    public override void OnStateExit() { }

    private IEnumerator QueueAttack()
    {
        // Locking the enemies destination to the player, getting them to attack
        queueFlag = true;
        AIBrain aiBrain = _unitSlots[_activeIndex].GetBrain();
        MoveAllRandomlyAroundPlayer();
        aiBrain.GetAIBehaviour("Movement").LockDestinationToPlayer(1.0f);

        // Waiting a certain amount of time
        yield return new WaitForSeconds(queueTime);

       // Debug.Log(_activeIndex);

        // Wrapping the index count
        if (_activeIndex >= _unitSlots.Count - 1)
            _activeIndex = 0;
        else
            _activeIndex++;

        queueFlag = false;

        // // Check if there are any enemies are left before proceeding
        // if (this.enemyGroupHandler.GetEnemies().Count <= 0)
        // {
        //     _activeIndex = 0;
        //     queueFlag = false;
        //     yield break;
        // }

        // // Move back into position
        // if (!aiBrain.GetHandler().IsParried() && aiBrain.GetHandler().GetFunctional())
        //     aiBrain.GetAIBehaviour("Movement").OverrideDestination(GetRandomizedPositionAroundCenter(_attackerLastPosition, Random.Range(minGroupWanderRadius, maxGroupWanderRadius)), 1.0f);

    }

    // Adds the active group of enemies into the unit slots
    private void UpdateUnitSlots()
    {
        /* Optimization note: 
                            Find a way to only create this list once.
                            This currently sorts every time the group enters this state,
                            this helps because once an enemy dies, we need to resort anyways.
                            So, we can refine this to only create the list and sort if
                            any enemies die.
        */

        // Creating a new list of enemies from the enemies within the group
        //_unitSlots = new List<EnemyHandler>(enemyGroupHandler.GetEnemies());

        _unitSlots.Clear();
        _unitSlots.AddRange(enemyGroupHandler.GetObjectPooler().GetActiveEnemyList());

        // Sorting the list; using a lambda function to compare the distance to the player
        _unitSlots.Sort((u1, u2) => u1.GetBrain().GetDistanceToPlayer().
                          CompareTo(u2.GetBrain().GetDistanceToPlayer()));
    }


    // Returns the distance from the closest enemy to the player
    public float GetFirstDistanceToPlayer()
    {
        if(_unitSlots.Count > 0)
        {
            return (_unitSlots[0].transform.position - enemyGroupHandler.playerTransform.position).magnitude;
        }
        else
        {
            //Debug.LogError("Group Compbat: Unit slots are empty!");
            return -1.0f;
        }
    }

    // Removes an enemy from the group
    public void RemoveFromUnitSlot(EnemyHandler enemy)
    {
        StopCoroutine(QueueAttack());
        UpdateUnitSlots();

        // Wrapping the index count
        if (_activeIndex >= _unitSlots.Count - 1)
            _activeIndex = 0;
        else
            _activeIndex++;
    }

    // Moves all enemys to a new position within the radius
    private void MoveAllRandomlyAroundPlayer()
    {
        foreach (EnemyHandler e in _unitSlots)
        {
            if (!e.IsParried() && e.GetFunctional() && !e.GetBrain().GetAIBehaviour("Movement").IsLockedOntoPlayer())
            {
                e.GetBrain().SetBehaviour("Idle");
                AIBehaviour movement = e.GetBrain().GetAIBehaviour("Movement");
                movement.OverrideDestination(GetRandomizedPositionAroundCenter(e.GetBrain().PlayerTransform.position, Random.Range(minGroupWanderRadius, maxGroupWanderRadius)), 1.0f);
            }
        }
    }

    // Returns a randomized position from the radius around the center of an object
    // This function will be replaced when "Unit Slotting" or "AI Group Control" gets implemented.
    private Vector3 GetRandomizedPositionAroundCenter(Vector3 center, float radius)
    {
        // create random angle between 0 to 360 degrees 
        float ang = Random.value * 360;
        Vector3 pos = Vector3.zero;
        pos.x = center.x + radius * Mathf.Sin(ang * Mathf.Deg2Rad);
        pos.y = center.y;
        pos.z = center.z + radius * Mathf.Cos(ang * Mathf.Deg2Rad);
        return pos;
    }
}
