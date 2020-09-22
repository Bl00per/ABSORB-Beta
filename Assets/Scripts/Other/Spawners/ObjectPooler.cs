using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ObjectPooler : MonoBehaviour
{
    /*
    Unity Heirachy:
        Enemy Group
            - Minions
                - Minion(1)
                - Minion(2)
                - Minion(3)
            - Specials
                - Special(1)
            - Elites
                - Elite(1)

        - Spawner:
            - Has multiple property flags, eg; spawnerActive = false; spawnTime = 1.0f, etc.
            - Has a flag to only spawn when not rendered by camera
                - Has an array of transforms which will be picked at random and based on if the camera can see them or not
            - Has a queue that the enemies will get stored in once they are "killed".
            - Spawns the enemy by activating and resetting the enemy.

        - Object Pool:
            - Hold the enemies within an array, all on standby.
            - Has a GetDeactivated() function that returns an array of all unactive enemies
            - Has a GetActivated() function that returns an array of all active enemies

            // enemies[0] // The first enemy which is deactivated
    */
    private List<EnemyHandler> _activeEnemies;
    private List<EnemyHandler> _inactiveEnemies;
    private EnemyGroupHandler _enemyGroupHandler;

    [Header("References")]
    public Transform playerTransform;

    [Header("Spawner Properties")]
    public List<EnemyHandler.EnemyType> enemyRespawnList;
    public bool spawnerActive = true;
    public float spawnTime = 1.0f;

    [Header("Enemy that will turn off the spawner on death")]
    public GameObject finalEnemy;

    [Space]
    public Transform[] spawnerPositions;
    private List<Transform> _spawnPointsOffScreen = new List<Transform>();

    private bool _isSpawning = false;

    // Called on initialise
    private void Awake()
    {
        // Initialising the enemy lists
        _activeEnemies = new List<EnemyHandler>();
        _inactiveEnemies = new List<EnemyHandler>();
        // Get the enemy group handler on this object
        _enemyGroupHandler = this.GetComponent<EnemyGroupHandler>();

        // Populate list of enemies with the children of this gameobject
        foreach (Transform child in transform)
        {
            foreach (Transform grandchild in child)
            {
                EnemyHandler enemy;
                enemy = grandchild.GetComponent<EnemyHandler>();
                enemy.SetEnemyGroupHandler(_enemyGroupHandler);
                _activeEnemies.Add(enemy);
                //_unactiveEnemies.ForEach((_unactiveEnemies) => { _unactiveEnemies.gameObject.SetActive(false); });
            }
        }
    }

    // Called every frame
    private void Update()
    {
        // Checking for specified enemy spawn
        CheckForEnemyTypeSpawn();
    }

    // Swaps the list which the specifed enemy is on
    public void SwapList(EnemyHandler enemyHandler)
    {
        // If the enemy is in the active list
        if (this.GetListStatus(enemyHandler))
        {
            enemyHandler.gameObject.SetActive(false);
            _activeEnemies.Remove(enemyHandler);
            _inactiveEnemies.Add(enemyHandler);
        }
        // If the enemy is in the inactive list
        else
        {
            enemyHandler.gameObject.SetActive(true);
            _activeEnemies.Add(enemyHandler);
            _inactiveEnemies.Add(enemyHandler);
        }
    }

    // Returns true if on the active list and false if not. Only call this if you expect the enemy to be within either group!
    public bool GetListStatus(EnemyHandler enemy)
    {
        if (_activeEnemies.Contains(enemy))
            return true;
        else if (_inactiveEnemies.Contains(enemy))
            return false;
        else
        {
            Debug.LogError("Object Pooler: Enemy not contained in either list.");
            return false;
        }
    }

    // Returns the list of active enemies
    public List<EnemyHandler> GetActiveEnemyList()
    {
        return _activeEnemies;
    }

    // Returns the list of unactive enemies
    public List<EnemyHandler> GetInactiveEnemyList()
    {
        return _inactiveEnemies;
    }

    // Finds and returns an inactive enemy based on the enemy type
    public EnemyHandler FindInactiveEnemy(EnemyHandler.EnemyType enemyType)
    {
        // Linear search for enemy type
        foreach (EnemyHandler enemy in _inactiveEnemies)
        {
            if (enemy.GetEnemyType() == enemyType)
                return enemy;
        }

        // No enemy found
        return null;
    }

    // Finds and returns an active enemy based on the enemy type
    public EnemyHandler FindActiveEnemy(EnemyHandler.EnemyType enemyType)
    {
        // Linear search for enemy type
        foreach (EnemyHandler enemy in _activeEnemies)
        {
            if (enemy.GetEnemyType() == enemyType)
                return enemy;
        }

        // No enemy found
        return null;
    }

    public void CheckForEnemyTypeSpawn()
    {
        // Check if the target enemy is still alive and count of inactive is > 0
        if ((finalEnemy == null || !finalEnemy.activeInHierarchy) && _inactiveEnemies.Count > 0 && !_isSpawning)
        {
            foreach (EnemyHandler enemy in _inactiveEnemies)
            {
                foreach (EnemyHandler.EnemyType enemyType in enemyRespawnList)
                {
                    if (enemy.GetEnemyType() == enemyType)
                    {
                        StartCoroutine(RespawnEnemy(enemy));
                        break;
                    }
                }
            }
        }
    }

    // Finds a spawn point not rendered by the camera, and spawns the specified enemy there
    public IEnumerator RespawnEnemy(EnemyHandler enemy)
    {
        // Setting the spawning flag to true
        _isSpawning = true;

        // Wait until for # amount of seconds before spawning the enemy
        yield return new WaitForSeconds(spawnTime);

        // Updating the spawn points which are off-screen
        UpdateSpawnPointsOffScreen();

        // Exiting this function if there are no points on-screen
        if (_spawnPointsOffScreen.Count <= 0)
        {
            // Setting the spawning flag to false
            _isSpawning = false;

            // Printing a debug message
            Debug.LogWarning("Object Pool: Couldn't find spawner off screen.");
            yield break;
        }

        // Get a random number between 0 and the spawn point max
        int spawnNumber = Random.Range(0, _spawnPointsOffScreen.Count);

        // Setting to the active list and enabling functionality
        SpawnFromPool(enemy, _spawnPointsOffScreen[spawnNumber].position, Quaternion.identity);

        // Setting the spawning flag to false
        _isSpawning = false;
    }

    private void UpdateSpawnPointsOffScreen()
    {
        _spawnPointsOffScreen.Clear();
        for (int i = 0; i < spawnerPositions.Length; ++i)
        {
            Vector3 viewSpacePos = Camera.main.WorldToViewportPoint(spawnerPositions[i].position);
            if (viewSpacePos.x <= 0 && viewSpacePos.y <= 1 && viewSpacePos.z <= 0)
                _spawnPointsOffScreen.Add(spawnerPositions[i]);
        }
    }

    public GameObject SpawnFromPool(EnemyHandler enemy, Vector3 position, Quaternion rotation)
    {
        // Remove the object from the inactive list
        SwapList(enemy);
        GameObject objectToSpawn = enemy.gameObject;

        // Basically activate the enemy
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        return objectToSpawn;
    }

    // [System.Serializable]
    // public class Pool
    // {
    //     public string tag;
    //     public GameObject prefab;
    //     public int size;
    // }

    // public List<Pool> pools;
    // public Dictionary<string, Queue<GameObject>> poolDictionary;
    // public Queue<GameObject> objectPool;
    // private SpawnerV2 spawner;

    // public static ObjectPooler Instance;

    // private void Awake()
    // {
    //     // Singleton
    //     Instance = this;

    //     poolDictionary = new Dictionary<string, Queue<GameObject>>();
    //     spawner = FindObjectOfType<SpawnerV2>();

    //     foreach (Pool pool in pools)
    //     {
    //         objectPool = new Queue<GameObject>();

    //         for (int i = 0; i < pool.size; i++)
    //         {
    //             GameObject obj = Instantiate(pool.prefab);
    //             AIBrain aIBrain = obj.GetComponent<AIBrain>();
    //             aIBrain.PlayerTransform = spawner.playerTransform;
    //             //aIBrain.GetComponent<EnemyHandler>().SetupSpawner(spawner);
    //             obj.SetActive(false);
    //             objectPool.Enqueue(obj);
    //         }

    //         poolDictionary.Add(pool.tag, objectPool);
    //     }
    // }

    // public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    // {
    //     // Throw a warning if there is no pool with that tag
    //     if (!poolDictionary.ContainsKey(tag))
    //     {
    //         Debug.LogWarning("Pool with tag " + tag + " doesn't exist");
    //         return null;
    //     }

    //     // Remove the object from the dictionary
    //     GameObject objectToSpawn = poolDictionary[tag].Dequeue();

    //     // Basically activate the enemy
    //     objectToSpawn.SetActive(true);
    //     objectToSpawn.transform.position = position;
    //     objectToSpawn.transform.rotation = rotation;

    //     //poolDictionary[tag].Enqueue(objectToSpawn);

    //     return objectToSpawn;
    // }

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
}
