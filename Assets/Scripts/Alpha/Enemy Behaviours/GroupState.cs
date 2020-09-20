using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GroupState : MonoBehaviour
{
    [Header("Flocking Properties")]
    /*
    Each boid flies towards the the other boids. 
    But they don't just immediately fly directly at each other. 
    They gradually steer towards each other at a rate that you can adjust with the "coherence" slider.
    */
    public float coherenceFactor = 1.0f;
    /*
    Each boid also tries to avoid running into the other boids. 
    If it gets too close to another boid it will steer away from it. 
    You can control how quickly it steers with the "separation" slider.
    */
    public float seperationFactor = 1.0f;
    /*
    Finally, each boid tries to match the vector (speed and direction) of the other boids around it. 
    Again, you can control how quickly they try to match vectors using the "coherence" slider.
    */
    public float alignmentFactor = 1.0f;

    // How far will the move distance be for each iteration?
    public float moveDistance = 500.0f;

    // Creating link between handler and state
    protected EnemyGroupHandler enemyGroupHandler;
    public void Initialise(EnemyGroupHandler enemyGroupHandler)
    {
        this.enemyGroupHandler = enemyGroupHandler;
    }

    // Functions to be overwritten by derived class
    abstract public void OnStateEnter();
    abstract public void OnStateUpdate();
    abstract public void OnStateFixedUpdate();
    abstract public void OnStateExit();
}
