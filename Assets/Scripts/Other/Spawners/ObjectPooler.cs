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
    */
    private List<EnemyHandler> _activeEnemies;
    private List<EnemyHandler> _respawnQueue;
    private List<EnemyHandler> _inactiveEnemies;
    private EnemyGroupHandler _enemyGroupHandler;

    [Header("References")]
    public Transform playerTransform;

    [Header("Spawner Properties")]
    public List<EnemyHandler.EnemyType> enemyRespawnList;
    public bool spawnerActive = true;
    public float spawnTime = 1.0f;
    public bool findOffScreenSpawnPoint = true;
    public Transform onScreenSpawnPoint;

    [Header("Reference to the final enemy of the combat sequence.")]
    public EnemyHandler finalEnemy;

    [Space]
    public Transform[] spawnerPositions;
    private List<Transform> _spawnPointsOffScreen = new List<Transform>();

    private bool _isSpawning = false;

    // Called on initialise
    private void Awake()
    {
        // Initialising the enemy lists
        _activeEnemies = new List<EnemyHandler>();
        _respawnQueue = new List<EnemyHandler>();
        _inactiveEnemies = new List<EnemyHandler>();

        // Get the enemy group handler on this object
        _enemyGroupHandler = this.GetComponent<EnemyGroupHandler>();

        // Populate list of enemies with the children of this gameobject
        foreach (Transform child in transform.GetChild(0))
        {
            foreach (Transform grandchild in child)
            {
                EnemyHandler enemy;
                enemy = grandchild.GetComponent<EnemyHandler>();
                enemy.SetEnemyGroupHandler(_enemyGroupHandler);
                _activeEnemies.Add(enemy);
            }
        }
        //_unactiveEnemies.ForEach((_unactiveEnemies) => { _unactiveEnemies.gameObject.SetActive(false); });
    }

    // Called every frame
    private void Update()
    {
        // Checking for specified enemy spawn
        CheckForEnemyTypeSpawn();
    }

    #region Spawner Functions

    // Ignores the queue
    public void SwapActiveLists(EnemyHandler enemyHandler)
    {
        // If the enemy is in the active list
        if (this.GetListStatus(enemyHandler))
        {
            if (enemyHandler.GetFunctional())
                enemyHandler.SetFunctional(false);
            _activeEnemies.Remove(enemyHandler);
            _inactiveEnemies.Add(enemyHandler);
        }
        // If the enemy is in the inactive list
        else
        {
            if (!enemyHandler.GetFunctional())
                enemyHandler.SetFunctional(true);
            _inactiveEnemies.Remove(enemyHandler);
            _activeEnemies.Add(enemyHandler);
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

    // Finds and returns an inactive enemy based on the enemy type
    public EnemyHandler FindEnemyTypeWithinQueue(EnemyHandler.EnemyType enemyType)
    {
        // Linear search for enemy type
        foreach (EnemyHandler enemy in _respawnQueue)
        {
            if (enemy.GetEnemyType() == enemyType)
                return enemy;
        }

        // No enemy found
        return null;
    }

    // Removes enemies from the queue
    public void RemoveFromQueue(EnemyHandler handler)
    {
        if (_respawnQueue.Contains(handler))
        {
            _respawnQueue.Remove(handler);
            _activeEnemies.Add(handler);

            if (!handler.GetFunctional())
                handler.SetFunctional(true);
        }
    }

    // Removes enemy from the inactive list and adds it to the queue
    public void AddToQueue(EnemyHandler handler)
    {
        if (!_respawnQueue.Contains(handler))
        {
            _inactiveEnemies.Remove(handler);
            _respawnQueue.Add(handler);
        }
    }

    // Returns true if the enemy is eligable for the queue
    public bool GetCanAddToQueue(EnemyHandler handler)
    {
        foreach (EnemyHandler.EnemyType enemyType in enemyRespawnList)
        {
            if (handler.GetEnemyType() == enemyType)
                return true;
        }
        return false;
    }

    public bool AddToInactiveList(EnemyHandler handler, bool addToQueue = true)
    {
        // Swaps the enemy from the active to the inactive list
        SwapActiveLists(handler);

        // Add to queue if we can
        if (addToQueue && GetCanAddToQueue(handler) && (finalEnemy == null || finalEnemy.IsAlive()))
        {
            AddToQueue(handler);
            return true;
        }

        // If not, just leave on the inactive list
        return false;
    }

    public void CheckForEnemyTypeSpawn()
    {
        // Check if the target enemy is still alive and count of inacti
        if ((finalEnemy == null || finalEnemy.IsAlive()) && _respawnQueue.Count > 0 && !_isSpawning && spawnerActive)
        {
            // Iterating through the inactive enemies list
            foreach (EnemyHandler enemy in _respawnQueue)
            {
                // If the types are a match, respawn the enemy
                StartCoroutine(RespawnEnemy(enemy));
                break;
            }
        }
        else if(specialEnemy != null && !_isSpawning) 
        {
            if(!specialEnemy.IsAlive())
            {
                StartCoroutine(RespawnEnemy(specialEnemy));
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

        if (findOffScreenSpawnPoint)
        {
            // Updating the spawn points which are off-screen
            UpdateSpawnPointsOffScreen();

            // Exiting this function if there are no points on-screen
            if (_spawnPointsOffScreen.Count <= 0)
            {
                // Setting the spawning flag to false
                _isSpawning = false;

                // Printing a debug message
                Debug.LogWarning("Object Pool - Couldn't find spawner off screen.");
                yield break;
            }
        }

        // Removing enemy from the queue
        RemoveFromQueue(enemy);

        // Refreshing the enemy list within the group handler
        _enemyGroupHandler.UpdateEnemyList();

        if (findOffScreenSpawnPoint)
        {
            // Get a random number between 0 and the spawn point max
            int spawnNumber = Random.Range(0, _spawnPointsOffScreen.Count);

            // Setting the enemy position and rotation to the spawn point
            enemy.transform.position = _spawnPointsOffScreen[spawnNumber].position;
            enemy.transform.rotation = _spawnPointsOffScreen[spawnNumber].rotation;
        }
        else
        {
            // Setting the enemy position and rotation to the spawn point
            enemy.transform.position = onScreenSpawnPoint.position;
            enemy.transform.rotation = onScreenSpawnPoint.rotation;
        }

        // Resetting the enemies properties
        enemy.Reset();

        // Setting the spawning flag to false
        _isSpawning = false;
    }

    private void UpdateSpawnPointsOffScreen()
    {
        _spawnPointsOffScreen.Clear();
        for (int i = 0; i < spawnerPositions.Length; ++i)
        {
            if (!IsTargetVisible(Camera.main, spawnerPositions[i].gameObject))
                _spawnPointsOffScreen.Add(spawnerPositions[i]);
        }
    }

    private bool IsTargetVisible(Camera c, GameObject go)
    {
        var planes = GeometryUtility.CalculateFrustumPlanes(c);
        var point = go.transform.position;
        foreach (var plane in planes)
        {
            if (plane.GetDistanceToPoint(point) < 0)
                return false;
        }
        return true;
    }

    public EnemyHandler GetActiveEnemy(int index)
    {
        return _activeEnemies[index];
    }

    #endregion
}