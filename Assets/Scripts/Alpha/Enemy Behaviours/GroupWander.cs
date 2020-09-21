using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupWander : GroupState
{
    [Header("References")]
    public Transform wanderRadiusCenter;

    [Header("Properties")]
    public float updatePathTime = 1.0f;
    public float wanderRadius = 15.0f;
    private bool _isWaiting = false;

    public override void OnStateEnter()
    {
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
        foreach (EnemyHandler e in enemyGroupHandler.GetEnemies())
        {
            e.GetBrain().SetBehaviour("Idle");
            AIBehaviour movement = e.GetBrain().GetAIBehaviour("Movement");
            movement.OverrideDestination(GetRandomizedPositionAroundCenter(wanderRadiusCenter.position, wanderRadius), 1.0f);
        }
    }

    // Moves one enemy at random
    private void MoveOne()
    {
        EnemyHandler randEnemy = this.enemyGroupHandler.GetEnemy(Random.Range(0, enemyGroupHandler.GetEnemies().Count));
        randEnemy.GetBrain().SetBehaviour("Idle");
        AIBehaviour movement = randEnemy.GetBrain().GetAIBehaviour("Movement");
        movement.OverrideDestination(GetRandomizedPositionAroundCenter(wanderRadiusCenter.position, wanderRadius), 1.0f);
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
