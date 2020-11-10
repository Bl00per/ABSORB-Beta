using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupRetreat : GroupState
{
    [Header("References")]
    public Transform retreatPosition; // The position the group will retreat to
    // This will default to the wander radius if not assigned

    [Header("Properties")]
    public float distanceFromPosition = 10.0f; // How far the group needs to be before entering wander state

    // Getting the group wander state on this gameobject
    private GroupWander _groupWander;
    private void Awake()
    {
        _groupWander = this.GetComponent<GroupWander>();
    }

    private void Start()
    {
        // Checking if the retreat position is null before assigning it to the wander position
        if(retreatPosition == null)
            retreatPosition = _groupWander.wanderRadiusCenter;
    }

    public override void OnStateEnter() {}

    public override void OnStateUpdate()
    {
        float dist = Vector3.Distance(enemyGroupHandler.GetCenterOfMass(), retreatPosition.position);
        Debug.Log(dist);
        // Checking the distance from the center of mass to the retreat destination
        if(Vector3.Distance(enemyGroupHandler.GetCenterOfMass(), retreatPosition.position) <= distanceFromPosition)
        {
            enemyGroupHandler.SetState(EnemyGroupHandler.GroupState.WANDER);
            return;
        }

        // Moving all enemies to destination
        foreach(EnemyHandler enemy in enemyGroupHandler.GetObjectPooler().GetActiveEnemyList())
            enemy.GetBrain().GetAIBehaviour("Movement").OverrideDestination(retreatPosition.position);
    }

    public override void OnStateFixedUpdate(){}

    public override void OnStateExit(){}
}
