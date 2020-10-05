// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
// using UnityEngine;

// public class SpawnerV2 : MonoBehaviour
// {
//     [Header("References")]
//     // The transform the enemy will target
//     public Transform playerTransform;
//     public string objectToSpawnTag;
//     [Header("Spawner Properties")]
//     public float timeBetweenEachEnemySpawn = 5f;
//     [Header("Only spawn the max number of enemies")]
//     public bool setSpawnAmount = false;
//     [Header("Keep off if you want to spawn first wave instantaneously")]
//     public bool waitForSpawn = false;
//     [Space]
//     public Transform[] spawnerPositions;

//     public int numberOfTaggedObjects;
//     private int _objectPoolCount;
//     private int _checkAmountOfSpawns;
//     private float _tempTime;

//     ObjectPooler objectPooler;

//     // Use this for initialization
//     void Start()
//     {
//         objectPooler = ObjectPooler.Instance;
//         _objectPoolCount = objectPooler.poolDictionary[objectToSpawnTag].Count;
//         _tempTime = timeBetweenEachEnemySpawn;

//         if (waitForSpawn)
//         {
//             timeBetweenEachEnemySpawn = 0.0f;
//             waitForSpawn = false;
//         }
//     }

//     // Update is called once per frame
//     void Update()
//     {
//         // TODO:
//         // - Potentially spawn more enemies if 1 dies (e.g. 1 enemy dies, 2 more spawn)

//         // If it hasnt reached the max amount of spawns, keep running or if it's set to false
//         if (setSpawnAmount && _checkAmountOfSpawns != _objectPoolCount || !setSpawnAmount)
//         {
//             // Make sure there is an object to spawn before running the coroutine
//             if (!waitForSpawn && objectPooler.poolDictionary[objectToSpawnTag].Count > 0)
//             {
//                 waitForSpawn = true;
//                 StartCoroutine(SpawnSequence());
//             }
//         }

//         // Keep track of the number of enemies currently active in the scene
//         numberOfTaggedObjects = GameObject.FindGameObjectsWithTag(objectToSpawnTag).Length;
//     }

//     IEnumerator SpawnSequence()
//     {
//         // Wait for set amount of seconds before we spawn a new enemy
//         yield return new WaitForSeconds(timeBetweenEachEnemySpawn);
//         timeBetweenEachEnemySpawn = _tempTime;
//         // Choose a random position from the array to spawn them at
//         int spawnNumber = Random.Range(0, spawnerPositions.Count());
//         objectPooler.SpawnFromPool(objectToSpawnTag, spawnerPositions[spawnNumber].transform.position, Quaternion.identity);
//         waitForSpawn = false;
        
//         // Increment to keep track of max amount of spawns
//         if (setSpawnAmount)
//             _checkAmountOfSpawns += 1;
//     }
// }
