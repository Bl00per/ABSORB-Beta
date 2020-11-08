using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupCombat : GroupState
{
    [Header("Group Movement Properties")]
    public float returnToChaseDistance = 12.0f;
    public float stopMovingToCircleDistance = 10.0f;

    [Header("Unit Slotting Properties")]
    public float queueTime = 1.5f;
    public float circleRadius = 7.5f;

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
        // If the player isn't alive, return to wander state
        if (!playerHandler.GetIsAlive())
        {
            enemyGroupHandler.SetState(EnemyGroupHandler.GroupState.WANDER);
        }

        if (_unitSlots.Count <= 0)
            return;

        // If the first index's distance from the player is greater than returnToChaseDistance, then return to chasing the player
        if (Vector3.Distance(_unitSlots[0].transform.position, enemyGroupHandler.playerTransform.position) >= returnToChaseDistance)
        {
            enemyGroupHandler.SetState(EnemyGroupHandler.GroupState.CHASE);
        }

        // Setting up an enemy for an attack
        if (!queueFlag)
            StartCoroutine(QueueAttack());

        if (Vector3.Distance(_unitSlots[0].transform.position, enemyGroupHandler.playerTransform.position) <= stopMovingToCircleDistance)
            return;

        // Iterating over each of the passive enemies (Currently only doing this for minions)
        for (int i = 0; i < _unitSlots.Count; ++i)
        {
            // Continuing loop if the active index is equal to the iterator
            if (i == _activeIndex)
                continue;

            // Continuing loop if enemy just attacked
            if (_unitSlots[i].GetJustAttacked())
                continue;

            // Get the enemy brain at this index
            AIBrain aiBrain = _unitSlots[i].GetBrain();

            //-Forcing the enemy to face the player
            _positionFix = aiBrain.PlayerTransform.position;
            _positionFix.y = aiBrain.transform.position.y;
            aiBrain.transform.forward = (_positionFix - aiBrain.transform.position).normalized;

            // Moving enemy to a new passive position
            aiBrain.GetAIBehaviour("Movement").OverrideDestination(GetPositionAroundPoint(enemyGroupHandler.playerTransform.position, i));
        }
    }

    public override void OnStateFixedUpdate() { }

    public override void OnStateExit() { }

    private IEnumerator QueueAttack()
    {
        // Setting the queue flag to true, meaning the enemy has started the attack.
        queueFlag = true;

        // Locking the enemies destination to the player, getting them to attack
        if (!_unitSlots[_activeIndex].GetJustAttacked())
            _unitSlots[_activeIndex].GetBrain().GetAIBehaviour("Movement").LockDestinationToPlayer(1.0f);
        else
        {
            IncreaseActiveIndex();
            queueFlag = false;
            yield break;
        }

        // Waiting a queue time before proceeding, giving the functionality of a timer
        yield return new WaitForSeconds(queueTime);

        // Increasing the active index
        IncreaseActiveIndex();

        // Setting the queue flag to false, meaning the enemy has either finished the attack, or we are ready to queue another attack.
        queueFlag = false;
    }

    public void IncreaseActiveIndex()
    {
        if (_activeIndex >= _unitSlots.Count - 1)
            _activeIndex = 0;
        else
            _activeIndex++;
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

        ObjectPooler objectPooler = enemyGroupHandler.GetObjectPooler();

        _unitSlots.Clear();
        for (int i = 0; i < objectPooler.GetActiveEnemyList().Count; ++i)
        {
            EnemyHandler enemy = objectPooler.GetActiveEnemy(i);
            if (enemy.GetEnemyType() == EnemyHandler.EnemyType.MINION)
                _unitSlots.Add(enemy);
        }
        //_unitSlots.AddRange(enemyGroupHandler.GetObjectPooler().GetActiveEnemyList());

        // Sorting the list; using a lambda function to compare the distance to the player
        _unitSlots.Sort((u1, u2) => u1.GetBrain().GetDistanceToPlayer().
                          CompareTo(u2.GetBrain().GetDistanceToPlayer()));
    }


    // Returns the distance from the closest enemy to the player
    public float GetFirstDistanceToPlayer()
    {
        return (_unitSlots[0].transform.position - enemyGroupHandler.playerTransform.position).magnitude;
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

    public Vector3 GetPositionAroundPoint(Vector3 position, int index)
    {
        float degreesPerIndex = 360f / this._unitSlots.Count;
        var offset = new Vector3(0f, 0f, circleRadius);
        return position + (Quaternion.Euler(new Vector3(0f, degreesPerIndex * index, 0f)) * offset);
    }
}
