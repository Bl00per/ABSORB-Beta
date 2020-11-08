using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupWander : GroupState
{
    [Header("References")]
    public Transform wanderRadiusCenter;
    public EnemyHandler[] enemiesToIgnore;

    [Header("Properties")]
    public float updatePathTime = 1.0f;
    public float wanderRadius = 15.0f;
    private bool _isWaiting = false;
    private List<EnemyHandler> _wanderingEnemies = new List<EnemyHandler>(); 

    public override void OnStateEnter()
    {

        // Checking if there is any enemies we want to ignore
        if(enemiesToIgnore != null && enemiesToIgnore.Length > 0)
            SortWanderingEnemies();

        // If not, the wander enemy list becomes the active enemy list within the object pooler
        else
            _wanderingEnemies = enemyGroupHandler.GetObjectPooler().GetActiveEnemyList();
        
        MoveAll();
    }

    public override void OnStateUpdate()
    {
        // Check if the enemy is waiting to update a path, update if not
        if (!_isWaiting)
            StartCoroutine(WaitBeforeUpdatingPath());
    }

    public override void OnStateFixedUpdate() { }
    public override void OnStateExit() { }

    // Move all enemies towards the destination after cooldown time
    private IEnumerator WaitBeforeUpdatingPath()
    {
        _isWaiting = true;
        yield return new WaitForSeconds(updatePathTime);
        MoveOne();
        _isWaiting = false;
    }

    // Moves all enemys to a new position within the radius
    private void MoveAll()
    {
        foreach (EnemyHandler e in _wanderingEnemies)
        {
            e.GetBrain().SetBehaviour("Idle");
            AIBehaviour movement = e.GetBrain().GetAIBehaviour("Idle");
            movement.OverrideDestination(GetRandomizedPositionAroundCenter(wanderRadiusCenter.position, wanderRadius));
        }
    }

    // Moves one enemy at random
    private void MoveOne()
    {
        if (_wanderingEnemies.Count > 0)
        {
            EnemyHandler randEnemy = _wanderingEnemies[Random.Range(0, _wanderingEnemies.Count)];
            randEnemy.GetBrain().SetBehaviour("Idle");

            AIBehaviour movement = randEnemy.GetBrain().GetAIBehaviour("Idle");
            movement.OverrideDestination(GetRandomizedPositionAroundCenter(wanderRadiusCenter.position, wanderRadius));
        }
    }

    // Works out which enemies to ignore when wandering
    private void SortWanderingEnemies()
    {
        foreach(EnemyHandler enemy in enemyGroupHandler.GetObjectPooler().GetActiveEnemyList())
        {
            // Check if enemy is in the ignore list
            bool canAdd = true;
            foreach(EnemyHandler enemy1 in enemiesToIgnore)
            {
                if(enemy == enemy1)
                {
                    canAdd = false;
                    break;
                }
            }

            // Add to the wander list if we can add
            if(canAdd)
                _wanderingEnemies.Add(enemy);

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
