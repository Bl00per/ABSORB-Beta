using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityManager : MonoBehaviour
{
    // public enum E_Ability
    // {
    //     NONE,
    //     HAMMER,
    //     COUNT,
    // }

    // [Header("Properties")]
    // // The current ability in use. Do NOT use this to change the current state. Use SetAbility() instead.
    // public E_Ability startingAbility = E_Ability.NONE;
    // private E_Ability _currentAbility;

    // // The players absorb force feild
    // public GameObject playerForceField;

    // // The last enemy we parried
    // private AIBrain _lastParriedEnemy = null;

    // // The abosrb ability
    // private Absorb _absorb;

    // // The players shield
    // private SpecialParryBlock _specialParryBlock;

    // // Input manager to checking if button is pressed
    // private InputManager _inputManager;

    // // Array to fill out ability dictionary with
    // [System.Serializable]
    // public struct AbilityInformation
    // {
    //     public E_Ability e_Ability;
    //     public Ability ability;
    // }
    // [Header("References")]
    // public AbilityInformation[] abilityInformation;

    // // Dictionary to access abilties via enum
    // private Dictionary<E_Ability, Ability> _abilityDictionary = new Dictionary<E_Ability, Ability>();

    // // Options to assist with debugging
    // [Header("Debug")]
    // public bool printCurrentState = false;

    // // Called on initialise
    // private void Awake()
    // {
    //     // Get the absorb ability
    //     _absorb = this.GetComponent<Absorb>();

    //     // Get the special parry block ability
    //     _specialParryBlock = this.GetComponent<SpecialParryBlock>();

    //     // Fill out dictionary
    //     foreach (AbilityInformation ai in abilityInformation)
    //         _abilityDictionary.Add(ai.e_Ability, ai.ability);

    //     // Initialise references within states
    //     foreach (Ability a in _abilityDictionary.Values)
    //         a.InitialiseAbility(this);

    //     // Set the current ability to the starting ability
    //     if(startingAbility != E_Ability.NONE)
    //         SetAbility(startingAbility);

    //     _inputManager = FindObjectOfType<InputManager>();
    // }

    // // Called every frame
    // private void Update()
    // {
    //     // Exiting function if shield is active or if the player doesn't have an active ability
    //     if (_specialParryBlock.shieldState == SpecialParryBlock.ShieldState.Shielding || _currentAbility == E_Ability.NONE)
    //         return;

    //     // Debug printing if option enabled
    //     if (printCurrentState)
    //         Debug.Log(_currentAbility + " TIME");

    //     // Checks if the player uses the ability. ( Can't activate while using abosrb or shield )
    //     if (!_abilityDictionary[_currentAbility].Active && !_absorb.IsActive())
    //     {
    //         // Using ability
    //         if (_inputManager.GetSpecialAttackButtonPress())
    //             _abilityDictionary[_currentAbility].Activate();
    //     }
    // }

    // // Sets the current ability
    // public void SetAbility(E_Ability ability)
    // {
    //     if(_currentAbility != E_Ability.NONE)
    //         _abilityDictionary[_currentAbility].OnExit();

    //     _currentAbility = ability;

    //     if (_currentAbility != E_Ability.NONE)
    //         _abilityDictionary[_currentAbility].OnEnter();
    // }

    // // Sets and gets the last parried enemy
    // public AIBrain LastParriedEnemy
    // {
    //     get { return _lastParriedEnemy; }
    //     set { _lastParriedEnemy = value; }
    // }


    // // Sets the abosrb components target
    // public void SetAbsorbTarget(AIBrain target)
    // {
    //     _absorb.TargetEnemy = target;
    // }

    // // Returns true if the player has an assigned ability
    // public bool IsActive()
    // {
    //     //Debug.Log("Ability Manager returing: " + !(_currentAbility == AbilityManager.E_Ability.NONE));
    //     return !(_currentAbility == AbilityManager.E_Ability.NONE);
    // }
}
