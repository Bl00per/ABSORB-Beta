using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGroupHandler : MonoBehaviour
{
    public enum E_GroupState
    {
        WANDER,     // All enemies wander around a target destination
        CHASE,      // All enemies chase the player to try get into combat range
        COMBAT,     // Unit slotting, the enemies attack the player in a combat scenerio fashion
        RETREAT,    // If there is only # of minions left, they will run in the opposite direction of the player
        COUNT,      // Count of all states
    }

    [Header("References")]
    public Transform playerTransform;

    [Header("Combat Engagement Properties")]
    public float engagementDistance = 10.0f;

    [Header("Retreat Properties")]
    public float retreatDistance = 5.0f;
    public float retreatTime = 1.0f;

    [Header("Debug")]
    public bool debugCurrentState = false;

    // FSM variables
    private GroupState[] _groupStates;
    [Header("READ ONLY")]
    [SerializeField] private E_GroupState _currentState;

    // List of enemies within group
    private List<EnemyHandler> _activeEnemies = new List<EnemyHandler>();

    // Reference to the group combat state
    private GroupCombat _groupCombat;

    // Reference to the object pooler
    private ObjectPooler _objectPooler;

    // The end destination of the group 
    protected Vector3 _targetDestination = Vector3.zero;

    // Vectors for calculating the flock destination (not updated every frame)
    private Vector3 _cDir, _sDir, _aDir, _CoM;

    // Flag to determine if the enemies should retreat
    private bool _shouldRetreat = false;
    private bool _startedRetreatSequence = false;
    private int _attackerID = 0;

    // Called on initialise
    private void Awake()
    {
        // Populate the group states array
        _groupStates = new GroupState[(int)E_GroupState.COUNT];
        _groupStates[(int)E_GroupState.WANDER] = this.GetComponent<GroupWander>();
        _groupStates[(int)E_GroupState.CHASE] = this.GetComponent<GroupChase>();
        _groupStates[(int)E_GroupState.COMBAT] = this.GetComponent<GroupCombat>();
        _groupStates[(int)E_GroupState.RETREAT] = this.GetComponent<GroupRetreat>();

        // Getting the references
        _groupCombat = this.GetComponent<GroupCombat>();
        _objectPooler = this.GetComponent<ObjectPooler>();

        // Initialise the parent child connection in all the group states
        foreach (GroupState s in _groupStates)
            s?.Initialise(this);
    }

    private void Start()
    {
        // Getting the object pools inactive list
        UpdateEnemyList();

        // Setting the group state to WANDER on entry
        SetState(E_GroupState.WANDER);
    }

    // Called every frame
    private void Update()
    {
        // Check if there are any active enemies, if not then enter the idle state
        if (_activeEnemies.Count <= 0)
            return;

        // Checking if we want all enemies to retreat
        if (_shouldRetreat)
            UpdateRetreat();

        // Updating the current state
        _groupStates[(int)_currentState].OnStateUpdate();

        // Printing debug
        if (debugCurrentState)
            Debug.Log(_currentState);
    }

    // Set the current state of the handler
    public void SetState(E_GroupState nextState)
    {
        // Setting the current state
        _groupStates[(int)_currentState].OnStateExit();
        _currentState = nextState;
        _groupStates[(int)_currentState].OnStateEnter();
    }

    // Adds an enemy to the group
    public void Add(EnemyHandler enemy) => _activeEnemies.Add(enemy);

    // Removes an enemy from the group
    public void Remove(EnemyHandler enemy, bool addToQueue = true)
    {
        _objectPooler.AddToInactiveList(enemy, addToQueue);
        _groupCombat.RemoveFromUnitSlot(enemy);
        UpdateEnemyList();
    }

    // Refills the current enemy list with the active enemy list within the object pool
    public void UpdateEnemyList()
    {
        // Getting the object pools inactive list
        _activeEnemies.Clear();
        _activeEnemies.AddRange(_objectPooler.GetActiveEnemyList());
    }

    // Update the target destination of the group
    public void SetTargetDestination(Vector3 position) => _targetDestination = position;

    // Returns a flock destination for the specified enemy
    public Vector3 GetFlockDestination(EnemyHandler enemy)
    {
        _cDir = CalculateCoherence(_CoM, enemy.transform.position); //incase bug: removed .normalized
        _sDir = CalculateSeperation(enemy);
        _aDir = CalculateAlignment(enemy.transform.position);
        return enemy.transform.position + (_cDir + _sDir + _aDir).normalized * _groupStates[(int)_currentState].moveDistance * Time.deltaTime;
    }

    // Updates flocking destination for every enemy within group
    public void UpdateAllFlockDestinations()
    {
        // If there are enemies within the list
        if (_activeEnemies.Count > 1)
        {
            // Going to update this every frame for now, but might have to delay it on a coroutine if there are perfomace issues!
            _CoM = CalculateCenterOfMass();
            foreach (EnemyHandler enemy in _activeEnemies)
                enemy.GetBrain().GetAIBehaviour("Movement").OverrideDestination(GetFlockDestination(enemy), 1.0f);
        }
        else if (_activeEnemies.Count > 0)
        {
            _activeEnemies[0].GetBrain().GetAIBehaviour("Movement").OverrideDestination(_targetDestination, 1.0f);
        }
    }

    // Returns the correct direction for the enemy to move towards, considering coherence
    private Vector3 CalculateCoherence(Vector3 centerOfMass, Vector3 currentPosition)
    {
        return (centerOfMass - currentPosition) * _groupStates[(int)_currentState].coherenceFactor;
    }

    // Returns the correct direction for the enemy to move towards, considering seperation
    private Vector3 CalculateSeperation(EnemyHandler enemy)
    {
        // Iterating over the enemies
        Vector3 result = Vector3.zero;
        foreach (EnemyHandler e in _activeEnemies)
        {
            // Making sure not the check the enemy with itself
            if (e == enemy)
                continue;

            // Calculating the serpation result
            if (Vector3.Distance(enemy.transform.position, e.transform.position) < _groupStates[(int)_currentState].seperationFactor)
                result -= (e.transform.position - enemy.transform.position);
        }

        // Returning the calculated result
        return result;
    }

    // Returns the correct direction for the enemy to move towards, considering alignment
    private Vector3 CalculateAlignment(Vector3 currentPosition)
    {
        return (_targetDestination - currentPosition).normalized * _groupStates[(int)_currentState].alignmentFactor;
    }

    // Returns the groups center of mass
    public Vector3 CalculateCenterOfMass()
    {
        // Iterate over each enemy and calculate the center of mass
        Vector3 result = Vector3.zero;
        float sumOfAllWeights = 0.0f;
        Rigidbody enemyRigidbody;
        foreach (EnemyHandler enemy in _activeEnemies)
        {
            enemyRigidbody = enemy.GetRigidbody();
            result += enemyRigidbody.worldCenterOfMass * enemyRigidbody.mass;
            sumOfAllWeights += enemyRigidbody.mass;
        }

        // Returning the center of mass
        return result /= sumOfAllWeights;
    }

    // Forces all active enemies to retreat / not attack
    public void ForceAllEnemiesToRetreat(EnemyHandler attacker)
    {
        _attackerID = attacker.gameObject.GetInstanceID();
        foreach (EnemyHandler enemy in _activeEnemies)
        {
            if (enemy.gameObject.GetInstanceID() == _attackerID)
                continue;

            enemy.ForceJustAttacked();
            _shouldRetreat = true;
        }
    }

    // Forces all enemies to move away from the player overtime
    public void UpdateRetreat()
    {
        if (!_startedRetreatSequence)
            StartCoroutine(RetreatSequence());

        Vector3 _positionFix = playerTransform.position;
        foreach (EnemyHandler enemy in _activeEnemies)
        {
            if (enemy.gameObject.GetInstanceID() == _attackerID)
                continue;

            //Forcing the enemy to face the player
            _positionFix.y = enemy.transform.position.y;
            enemy.transform.forward = (_positionFix - enemy.transform.position).normalized;

            enemy.GetBrain().GetAIBehaviour("Movement").OverrideDestination(enemy.transform.position - (enemy.GetBrain().GetDirectionToPlayer() * retreatDistance * Time.deltaTime), 1.0f);
        }
    }

    // Coroutine to stop the enemies retreating
    private IEnumerator RetreatSequence()
    {
        _startedRetreatSequence = true;
        yield return new WaitForSeconds(retreatTime);
        _shouldRetreat = false;
        _startedRetreatSequence = false;
    }

    // Returns the calculated center of mass
    public Vector3 GetCenterOfMass() => _CoM;

    // Returns the array list of enemies
    public List<EnemyHandler> GetEnemies() => _activeEnemies;

    // Returns a specified enemy within the active enemies list
    public EnemyHandler GetActiveEnemy(int index) => _activeEnemies[index];

    // Returns the amount of enemies within group
    public int GetEnemyCount() => _groupStates.Length;

    // Returns the distance from the center of the group and the player
    public float GetCOMDistanceFromPlayer() => Vector3.Distance(CalculateCenterOfMass(), playerTransform.position);

    // Returns the object pool attached to the same object as this component
    public ObjectPooler GetObjectPooler() => _objectPooler;
}
