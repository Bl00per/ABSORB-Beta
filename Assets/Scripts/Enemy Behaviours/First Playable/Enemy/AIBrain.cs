using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
public class AIBrain : MonoBehaviour
{
    // Array list of information to fill the dictionary with
    [System.Serializable]
    public struct AIBehaviourInfo
    {
        public string name;
        public AIBehaviour behaviour;
    }
    [Header("References")]
    public AIBehaviourInfo[] behaviourInformation;

    // The public list of behaviours setup by user
    public Dictionary<string, AIBehaviour> _aiBehaviours = new Dictionary<string, AIBehaviour>();

    // Options to assist with debugging
    [Header("Options")]
    public bool printCurrentState = false;
    public string currentStateReadOnly = "NULL";
    [Range(0, 1)]
    public float pathUpdateCooldownTime = 0.2F;

    // Rigidbody attached to this object
    private Rigidbody _rigidbody;

    // Transform attached to this object
    private Transform _transform;

    // This enemy's handler.
    private EnemyHandler _handler;

    // This enemy's Nav Mesh Agent component
    private NavMeshAgent _navMeshAgent;

    // The transom the enemy will target
    private Transform _playerTransform;

    // The position which the enemy will target
    private Vector3 _targetDestination = Vector3.zero;

    // Current state ID of the fsm
    private string _currentBehaviourID = "";

    // Last state ID; used to check what state we were in last
    private string _lastStateID = "";

    // Called on initialise
    private void Awake()
    {
        // Getting required compoents
        _rigidbody = this.GetComponent<Rigidbody>();
        _transform = this.GetComponent<Transform>();
        _handler = this.GetComponent<EnemyHandler>();
        _navMeshAgent = this.GetComponent<NavMeshAgent>();

        // Find the player's transfrom
        _playerTransform = FindObjectOfType<PlayerHandler>().GetComponent<Transform>();

        // Fill out dictionary
        foreach (AIBehaviourInfo bi in behaviourInformation)
            _aiBehaviours.Add(bi.name, bi.behaviour);

        // Initialise references within states
        foreach (AIBehaviour aib in _aiBehaviours.Values)
            aib.InitialiseState(this);

        // Set current behaviour state
        _currentBehaviourID = behaviourInformation[0].name;
    }

    // Called every frame
    private void Update()
    {
        // Debug option to print current state
        if (printCurrentState)
            DebugStateMachine();

        _aiBehaviours[_currentBehaviourID].OnStateUpdate();
    }

    // Called every fixed update
    private void FixedUpdate()
    {
        _aiBehaviours[_currentBehaviourID].OnStateFixedUpdate();
    }

    // Sets the current state to the next state, calling exit/enter functions
    internal void SetBehaviour(string behaviour)
    {
        // Absorb quick fix: if in end state of machine, do NOT change state.
        if (/*_currentBehaviourID == "Absorbed" || */_currentBehaviourID == behaviour)
            return;

        // Setting the last state to be the current state before changing
        _lastStateID = _currentBehaviourID;

        // Checking if the current state we are about to leave is the attack state
        if (_lastStateID == "Attack")
        {
            // Setting the just attacked bool to true, and setting it false on a coroutine 
            _handler.SetJustAttacked(true);
            StartCoroutine(_handler.Coroutine_JustAttacked());
        }

        // Call OnExit() before the state switch, then call OnEnter() after.
        _aiBehaviours[_currentBehaviourID].OnStateExit();

        _currentBehaviourID = behaviour;
        _aiBehaviours[_currentBehaviourID].OnStateEnter();

        // Setting the new current state ID for debugging purposes.
        currentStateReadOnly = _currentBehaviourID;
    }

    // Updates the current destination to the players, if distance is over padding; only after cooldown time
    internal void SetDestinationOnCooldown(Vector3 targetDestination, float padding)
    {
        if (Vector3.Distance(_targetDestination, targetDestination) > padding)
            StartCoroutine(SetDestination(targetDestination));
    }

    // Updates the current destination to the players, if distance is over padding; does instantly
    internal void SetDestination(Vector3 targetDestination, float padding)
    {
        if (Vector3.Distance(_targetDestination, targetDestination) > padding && _handler.GetFunctional())
        {
            _targetDestination = targetDestination;
            _navMeshAgent.SetDestination(_targetDestination);
        }
    }

    // Waits a cooldown period before updating path, 
    public IEnumerator SetDestination(Vector3 targetDestination)
    {
        yield return new WaitForSeconds(pathUpdateCooldownTime);

        if (_handler.GetFunctional())
        {
            _navMeshAgent.SetDestination(_targetDestination);
            _targetDestination = targetDestination;
        }
    }

    // Returns the distance from this enemy and the player
    internal float GetDistanceToPlayer()
    {
        return Vector3.Distance(_transform.position, _playerTransform.position);
    }

    // Returns the direction from this enemy to the player
    internal Vector3 GetDirectionToPlayer()
    {
        return (_playerTransform.position - _transform.position).normalized;
    }

    // Returns the Rigidbody attached to this enemy
    internal Rigidbody GetRigidbody()
    {
        return _rigidbody;
    }

    // Returns the Transform attached to this enemy
    internal Transform GetTransform()
    {
        return _transform;
    }

    // Returns the EnemyHandler attached to this enemy
    internal EnemyHandler GetHandler()
    {
        return _handler;
    }

    // Returns the NavMeshAgent attached to this enemy
    internal NavMeshAgent GetNavMeshAgent()
    {
        return _navMeshAgent;
    }

    // Returns the ID of the state we were last in
    internal string GetLastStateID()
    {
        return _lastStateID;
    }

    // Returns the player's transform
    public Transform PlayerTransform
    {
        get { return _playerTransform; }
        set { _playerTransform = value; }
    }

    // Prints a debug message to unity's console
    public void DebugStateMachine()
    {
        Debug.Log(_currentBehaviourID);
    }

    // Returns a specified state of the brain
    public AIBehaviour GetAIBehaviour(string behaviour)
    {
        return _aiBehaviours[behaviour];
    }
}

