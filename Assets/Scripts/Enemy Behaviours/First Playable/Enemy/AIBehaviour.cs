using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIBehaviour : MonoBehaviour
{
    protected AIBrain brain;
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
    protected Rigidbody rigidbody;
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword
    protected Transform player;
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
    protected Transform transform;
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword
    protected EnemyHandler enemyHandler;
    protected Vector3 currentDestination = Vector3.zero;
    protected bool destinationLockedToPlayer = false;

    public void InitialiseState(AIBrain brain)
    {
        this.brain = brain;
        this.player = brain.PlayerTransform;
        this.rigidbody = brain.GetRigidbody();
        this.transform = brain.GetTransform();
        this.enemyHandler = brain.GetHandler();
    }

    abstract public void OnStateEnter();
    abstract public void OnStateUpdate();
    abstract public void OnStateFixedUpdate();
    abstract public void OnStateExit();

    // Moves the nav mesh agent to the new destination, also unlocking the "locked to player" flag
    public void OverrideDestination(Vector3 newDestination, float newDestinationPadding = 1.0f)
    {
        destinationLockedToPlayer = false;
        currentDestination = newDestination;
        brain.SetDestination(this.currentDestination, newDestinationPadding);
    }

    // Locks the nav mesh agents destination to the player
    public void LockDestinationToPlayer(float newDestinationPadding = 1.0f)
    {
        destinationLockedToPlayer = true;
        currentDestination = brain.PlayerTransform.position;
        brain.SetDestination(this.currentDestination, newDestinationPadding);
    }

    // Returns true if the enemy is locked onto the player
    public bool IsLockedOntoPlayer() => destinationLockedToPlayer;
}
