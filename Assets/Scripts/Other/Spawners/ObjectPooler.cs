using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ObjectPooler : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;
    public Queue<GameObject> objectPool;
    private SpawnerV2 spawner;

    public static ObjectPooler Instance;

    private void Awake()
    {
        // Singleton
        Instance = this;

        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        spawner = FindObjectOfType<SpawnerV2>();

        foreach (Pool pool in pools)
        {
            objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                AIBrain aIBrain = obj.GetComponent<AIBrain>();
                aIBrain.PlayerTransform = spawner.playerTransform;
                //aIBrain.GetComponent<EnemyHandler>().SetupSpawner(spawner);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        // Throw a warning if there is no pool with that tag
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist");
            return null;
        }

        // Remove the object from the dictionary
        GameObject objectToSpawn = poolDictionary[tag].Dequeue();

        // Basically activate the enemy
        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;
        
        //poolDictionary[tag].Enqueue(objectToSpawn);

        return objectToSpawn;
    }
}
