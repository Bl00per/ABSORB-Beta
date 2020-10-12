using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupCombat : GroupState
{
    [Header("Group Movement Properties")]
    public float returnToChaseDistance = 12.0f;

    [Header("Unit Slotting Properties")]
    public float queueTime = 1.5f;
    public int returnToPassiveRadiusMin = 10;
    public int returnToPassiveRadiusMax = 15;

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
        // Setting up an enemy for an attack
        if (!queueFlag && _unitSlots.Count > 0)
            StartCoroutine(QueueAttack());

        // Iterating over each of the passive enemies
        for (int i = 0; i < _unitSlots.Count; ++i)
        {
            // Continuing loop if the active index is equal to the iterator
            if (i == _activeIndex)
                return;

            // Get the enemy brain at this index
            AIBrain aiBrain = _unitSlots[i].GetBrain();

            // Forcing the enemy to face the player
            _positionFix = aiBrain.PlayerTransform.position;
            _positionFix.y = aiBrain.transform.position.y;
            aiBrain.transform.forward = (_positionFix - aiBrain.transform.position).normalized;

            //Vector3 position = GetPassivePosition(aiBrain.PlayerTransform.position, Random.Range(returnToPassiveRadiusMin, returnToPassiveRadiusMax));

            // Moving enemy to a new passive position
            //aiBrain.GetAIBehaviour("Movement").OverrideDestination(position, 1.0f);
        }
    }

    public override void OnStateFixedUpdate() { }

    public override void OnStateExit() { }

    private IEnumerator QueueAttack()
    {
        // Setting the queue flag to true, meaning the enemy has started the attack.
        queueFlag = true;

        // Locking the enemies destination to the player, getting them to attack
        if(_unitSlots[_activeIndex].GetEnemyType() == EnemyHandler.EnemyType.MINION)
            _unitSlots[_activeIndex].GetBrain().GetAIBehaviour("Movement").LockDestinationToPlayer(1.0f);

        // Waiting a queue time before proceeding, giving the functionality of a timer
        yield return new WaitForSeconds(queueTime);

        // Debug.Log("Just attacked: " + _activeIndex);

        // Wrapping the index count
        if (_activeIndex >= _unitSlots.Count - 1)
            _activeIndex = 0;
        else
            _activeIndex++;

        // Setting the queue flag to false, meaning the enemy has either finished the attack, or we are ready to queue another attack.
        queueFlag = false;
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

    private Vector3 GetPassivePosition(Vector3 center, float radius)
    {
        float ang = Random.value * 90;
        Vector3 pos = Vector3.zero;
        pos.x = center.x + radius * Mathf.Sin(ang * Mathf.Deg2Rad);
        pos.y = center.y;
        pos.z = center.z + radius * Mathf.Cos(ang * Mathf.Deg2Rad);
        return pos;
    }
}
