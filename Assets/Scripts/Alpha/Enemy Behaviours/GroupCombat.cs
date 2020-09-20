using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupCombat : GroupState
{
    [Header("Properties")]
    public float beginHuddleDistance = 20.0f;
    public float huddleDistance = 10.0f;
    public float queueTime = 1.5f;
    private bool queueFlag = false;
    private Vector3 _positionFix;
    private List<EnemyHandler> _unitSlots;
    private int _activeIndex = 0;

    public override void OnStateEnter()
    {
        // Sorting unit slots by closest to player
        SortUnitList();
        _activeIndex = 0;
    }

    public override void OnStateUpdate()
    {
        if (enemyGroupHandler.GetEnemies().Count == 1)
        {
            // Get the enemy brain at this index
            AIBrain aiBrain = enemyGroupHandler.GetEnemy(0).GetBrain();

            // Forcing the enemy to face the player
            _positionFix = aiBrain.PlayerTransform.position;
            _positionFix.y = aiBrain.transform.position.y;
            aiBrain.transform.forward = (_positionFix - aiBrain.transform.position).normalized;

            aiBrain.GetAIBehaviour("Movement").LockDestinationToPlayer(1.0f);
            return;
        }

        if (this.enemyGroupHandler.GetCOMDistanceFromPlayer() <= beginHuddleDistance)
        {
            // Queue slot for attack
            if (!queueFlag)
                StartCoroutine(QueueAttack());

            // Iterating over the enemy list within the group handler
            for (int i = 0; i < this.enemyGroupHandler.GetEnemies().Count; ++i)
            {
                // Get the enemy brain at this index
                AIBrain aiBrain = enemyGroupHandler.GetEnemy(i).GetBrain();

                // Checking if the enemy isn't attacking the player
                if (!aiBrain.GetAIBehaviour("Movement").IsLockedOntoPlayer() && !aiBrain.GetHandler().IsParried())
                {
                    // Forcing the enemy to face the player
                    _positionFix = aiBrain.PlayerTransform.position;
                    _positionFix.y = aiBrain.transform.position.y;
                    aiBrain.transform.forward = (_positionFix - aiBrain.transform.position).normalized;

                    // Move enemy into position around the player according to index
                    aiBrain.GetAIBehaviour("Movement").OverrideDestination(GetPositionAroundPlayer(aiBrain, i), 1.0f);
                }
            }
        }
        else
        {
            // Returning to chase if not in distance
            enemyGroupHandler.SetState(EnemyGroupHandler.E_GroupState.CHASE);
        }
    }

    public override void OnStateFixedUpdate() { }

    public override void OnStateExit() { }

    public Vector3 GetPositionAroundPlayer(AIBrain enemy, int index)
    {
        float degreesPerIndex = 360f / this.enemyGroupHandler.GetEnemies().Count;
        var pos = this.enemyGroupHandler.playerTransform.position;
        var offset = new Vector3(0f, 0f, huddleDistance + enemy.GetNavMeshAgent().stoppingDistance);
        return pos + (Quaternion.Euler(new Vector3(0f, degreesPerIndex * index, 0f)) * offset);
    }

    private IEnumerator QueueAttack()
    {
        if (_unitSlots[_activeIndex].GetBrain().GetHandler().IsParried())
            yield break;

        // Attacking the player 
        queueFlag = true;
        _unitSlots[_activeIndex].GetBrain().GetAIBehaviour("Movement").LockDestinationToPlayer(1.0f);

        // Waiting a certain amount of time
        yield return new WaitForSeconds(queueTime);

        // Check if there are any enemies are left before proceeding
        if (this.enemyGroupHandler.GetEnemies().Count <= 0)
        {
            _activeIndex = 0;
            queueFlag = false;
            yield break;
        }

        // Move enemy into position and make them face player
        if (!_unitSlots[_activeIndex].IsParried())
            _unitSlots[_activeIndex].GetBrain().GetAIBehaviour("Movement").OverrideDestination(GetPositionAroundPlayer(_unitSlots[_activeIndex].GetBrain(), _activeIndex), 1.0f);

        // Wrapping the index count
        if (_activeIndex >= _unitSlots.Count - 1)
            _activeIndex = 0;
        else
            _activeIndex++;

        queueFlag = false;
    }

    public void SortUnitList()
    {
        _unitSlots = new List<EnemyHandler>(enemyGroupHandler.GetEnemies());
        _unitSlots.Sort((u1, u2) => u1.GetBrain().GetDistanceToPlayer().CompareTo(u2.GetBrain().GetDistanceToPlayer()));
    }

    public void RemoveFromUnitSlot(EnemyHandler enemy)
    {
        StopCoroutine(QueueAttack());
        _unitSlots.Remove(enemy);
        SortUnitList();

        // Wrapping the index count
        if (_activeIndex >= _unitSlots.Count - 1)
            _activeIndex = 0;
        else
            _activeIndex++;
    }
}
