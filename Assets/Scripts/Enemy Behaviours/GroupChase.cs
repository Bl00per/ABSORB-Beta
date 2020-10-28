using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupChase : GroupState
{
    [Header("Combat Transition Properties")]
    public float distanceToEnterCombat = 10.0f;
    public float returnToWanderDistance = 12.0f;
    private List<EnemyHandler> _unitSlots = new List<EnemyHandler>();

    public override void OnStateEnter()
    {
        UpdateUnitSlots();
    }

    public override void OnStateUpdate()
    {
        // If the group is too far from the player, enter back into chase state
        if (!playerHandler.GetIsAlive())
        {
            enemyGroupHandler.SetState(EnemyGroupHandler.E_GroupState.WANDER);
            return;
        }

        if(_unitSlots.Count <= 0)
            return;

        // Move all enemies towards the player
        this.enemyGroupHandler.SetTargetDestination(enemyGroupHandler.playerTransform.position);
        this.enemyGroupHandler.UpdateAllFlockDestinations();

        // Check the distance from the closet enemy to the player
        if (Vector3.Distance(_unitSlots[0].transform.position, this.enemyGroupHandler.playerTransform.position) < distanceToEnterCombat)
            this.enemyGroupHandler.SetState(EnemyGroupHandler.E_GroupState.COMBAT);
    }

    // Adds the active group of enemies into the unit slots
    private void UpdateUnitSlots()
    {
        _unitSlots.Clear();
        _unitSlots.AddRange(enemyGroupHandler.GetObjectPooler().GetActiveEnemyList());

        // Sorting the list; using a lambda function to compare the distance to the player
        _unitSlots.Sort((u1, u2) => u1.GetBrain().GetDistanceToPlayer().
                          CompareTo(u2.GetBrain().GetDistanceToPlayer()));
    }

    public override void OnStateFixedUpdate() { }

    public override void OnStateExit() { }
}
