using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGroupManager : MonoBehaviour
{
    [Header("References")]
    public Transform playerTransform;


    [Header("Flocking Properties")]
    public float coherenceFactor = 1.0f;
    /*
    Each boid flies towards the the other boids. 
    But they don't just immediately fly directly at each other. 
    They gradually steer towards each other at a rate that you can adjust with the "coherence" slider.
    */
    public float seperationFactor = 1.0f;
    /*
    Each boid also tries to avoid running into the other boids. 
    If it gets too close to another boid it will steer away from it. 
    You can control how quickly it steers with the "separation" slider.
    */
    public float alignmentFactor = 1.0f;
    /*
    Finally, each boid tries to match the vector (speed and direction) of the other boids around it. 
    Again, you can control how quickly they try to match vectors using the "coherence" slider.
    */
    public float moveDistance = 5.0f;

    // List of enemies within group
    List<EnemyHandler> enemies = new List<EnemyHandler>();

    // The end destination of the group 
    private Vector3 _targetDestination = Vector3.zero;

    // Creating some vectors for caluclating
    private Vector3 cDir, sDir, aDir, calculatedResult, result, CoM;

    // Called on initialise
    private void Awake()
    {
        // Populate list of enemies with the children of this gameobject
        foreach (Transform child in transform)
            enemies.Add(child.GetComponent<EnemyHandler>());
    }

    // Called every frame
    private void Update()
    {
        _targetDestination = playerTransform.position;

        // If there are enemies within the list
        if (enemies.Count > 0)
        {
            // Going to update this every frame for now, but might have to delay it on a coroutine if there are perfomace issues!
            CoM = GetCenterOfMass();
            foreach (EnemyHandler enemy in enemies)
            {
                cDir = CalculateCoherence(CoM, enemy.transform.position).normalized;
                sDir = CalculateSeperation(enemy);
                aDir = CalculateAlignment(enemy.transform.position);
                calculatedResult = (cDir + sDir + aDir).normalized;
                result = enemy.transform.position + calculatedResult * moveDistance * Time.deltaTime;
                enemy.GetBrain().GetAIBehaviour("Movement").OverrideDestination(result, 1.0f);
            }
        }
    }

    // Adds an enemy to the group
    public void Add(EnemyHandler enemy)
    {
        enemies.Add(enemy);
    }

    // Removes an enemy from the group
    public void Remove(EnemyHandler enemy)
    {
        enemies.Remove(enemy);
    }

    // Returns the correct direction for the enemy to move towards, considering coherence
    private Vector3 CalculateCoherence(Vector3 centerOfMass, Vector3 currentPosition)
    {
        return (centerOfMass - currentPosition) * coherenceFactor;
    }

    // Returns the correct direction for the enemy to move towards, considering seperation
    private Vector3 CalculateSeperation(EnemyHandler enemy)
    {
        // Iterating over the enemies
        Vector3 result = Vector3.zero;
        foreach (EnemyHandler e in enemies)
        {
            // Making sure not the check the enemy with itself
            if (e == enemy)
                continue;

            // Calculating the serpation result
            if (Vector3.Distance(enemy.transform.position, e.transform.position) < seperationFactor)
                result -= (e.transform.position - enemy.transform.position);
        }

        // Returning the calculated result
        return result;
    }

    // Returns the correct direction for the enemy to move towards, considering alignment
    private Vector3 CalculateAlignment(Vector3 currentPosition)
    {
        return (_targetDestination - currentPosition).normalized * alignmentFactor;
    }

    // Returns the groups center of mass
    public Vector3 GetCenterOfMass()
    {
        // Iterate over each enemy and calculate the center of mass
        Vector3 result = Vector3.zero;
        float sumOfAllWeights = 0.0f;
        Rigidbody enemyRigidbody;
        foreach (EnemyHandler enemy in enemies)
        {
            enemyRigidbody = enemy.GetRigidbody();
            result += enemyRigidbody.worldCenterOfMass * enemyRigidbody.mass;
            sumOfAllWeights += enemyRigidbody.mass;
        }

        // Returning the center of mass
        return result /= sumOfAllWeights;
    }

    // Update the target destination of the group
    public void SetTargetDestination(Vector3 position)
    {
        _targetDestination = position;
    }
}
