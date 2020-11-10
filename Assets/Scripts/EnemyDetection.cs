using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDetection : MonoBehaviour
{
   
    public static List<Transform> enemies = new List<Transform>();

    
    public GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public static Transform GetClosestEnemy(List<Transform> enemies, Transform fromThis)
    {
        Transform bestTarget = null;

        float closestDistanceSqr = Mathf.Infinity;

        Vector3 currentPosition = fromThis.position;

        foreach (Transform potentialTarget in enemies)
        {
            
            Vector3 directionToTarget = potentialTarget.position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;

            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                bestTarget = potentialTarget;
            }
        }
        return bestTarget;


    }
   

    // Update is called once per frame
    void Update()
    {
        transform.position = player.transform.position;


    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {

            if (!enemies.Contains(other.transform))
            {
               
                enemies.Add(other.transform);
                
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {


            if (enemies.Contains(other.transform))
            {
                enemies.Remove(other.transform);
            }
        }

    }
    
}
