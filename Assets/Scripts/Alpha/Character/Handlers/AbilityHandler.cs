using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AbilitySickle))]
[RequireComponent(typeof(AbilityHammer))]
[RequireComponent(typeof(AbilityPot))]
public class AbilityHandler : MonoBehaviour
{
    public enum AbilityType
    {
        NONE,
        SICKLE,
        HAMMER,
        POT,
    }

    [Header("Parried Check Properties")]
    public float parriedCastRadius = 5.0f;

    [Header("Arm Skin Mesh Renderers")]
    public List<SkinnedMeshRenderer> abilityArms = new List<SkinnedMeshRenderer>();
    private int _abilityArmIndex = 0;

    [Header("Debug Options")]
    public AbilityType startingAbility = AbilityType.NONE;

    private Ability[] _abilities;
    private AbilityType _currentAbility = AbilityType.NONE;
    private List<Collider> _sortedHitList = new List<Collider>();
    private PlayerHandler _playerHandler;
    private InputManager _inputManager;
    private LocomotionHandler _locomotionHanlder;
    private Animator _animator;
    private bool _isAbosrbing = false;

    // Called on initialise
    private void Awake()
    {
        // Getting the player handler
        _playerHandler = this.GetComponent<PlayerHandler>();

        // Assign all the abilites
        _abilities = new Ability[(int)AbilityType.POT + 1];
        _abilities[(int)AbilityType.NONE] = null;
        _abilities[(int)AbilityType.SICKLE] = this.GetComponent<AbilitySickle>();
        _abilities[(int)AbilityType.HAMMER] = this.GetComponent<AbilityHammer>();
        _abilities[(int)AbilityType.POT] = this.GetComponent<AbilityPot>();

        // Initialising the handler-child connection
        foreach (Ability a in _abilities)
        {
            if (a != null)
                a.Initialise(this);
        }
    }

    // Called on initialise
    private void Start()
    {
        // Getting the references out of the player handler
        _inputManager = _playerHandler.GetInputManager();
        _locomotionHanlder = _playerHandler.GetLocomotionHandler();
        _animator = _playerHandler.GetAnimator();

        // Setting the starting ability
        SetAbility(startingAbility);
    }

    // Called every frame
    private void Update()
    {
        // Check for abosrb
        if (_currentAbility == AbilityType.NONE)
        {
            // Checking if the player requests to absorb
            CheckForAbosrb();

            // Returning out of this update function
            return;
        }
        else
        {
            // Checking if the player requests to acitvate their current ability
            if (_inputManager.GetSpecialAttackButtonPress() && !_isAbosrbing)
            {
                if (!_abilities[(int)_currentAbility].IsActive())
                    _abilities[(int)_currentAbility].Activate();
            }
        }
    }

    // Gets called every frame we don't have an ability
    private void CheckForAbosrb()
    {
        // Checking if player has inputed the absorb
        if (_inputManager.GetSpecialAttackButtonPress() && !_isAbosrbing)
        {
            // Get the closest enemy, then set them to the absorbed state
            EnemyHandler enemy = GetClosestParriedEnemy();
            if (enemy != null)
            {
                // Setting the absorb flags
                _isAbosrbing = true;
                _animator.SetBool("Absorb", true);

                // Activate slowdown
                _playerHandler.GetLocomotionHandler().Key_ActivateSlowdown();

                // If the player is shielding, then deactivate the shield
                if (_playerHandler.GetCombatHandler().shieldState == CombatHandler.ShieldState.Shielding)
                {
                    _playerHandler.GetCombatHandler().shieldState = CombatHandler.ShieldState.Cooldown;
                    _playerHandler.GetCombatHandler().shieldMeshRenderer.enabled = false;
                    _playerHandler.GetCombatHandler().shieldSphereCollider.enabled = false;
                    _playerHandler.GetCombatHandler().SetCanShield(false);
                }

                // Absorb enemies ability
                enemy.GetBrain().SetBehaviour("Absorbed");
                enemy.GetEnemyGroupHandler()?.Remove(enemy);
                this.SetAbility(enemy.GetAbilityType());
            }
        }
    }

    // Key Event: Deactivates abosrb once activated; only to be called through animation
    public void Key_DeactivateAbsorb()
    {
        //Debug.Log("Deactivating absorb");
        _animator.SetBool("Absorb", false);
        _playerHandler.GetLocomotionHandler().Key_DeactivateSlowdown();
        _isAbosrbing = false;
    }

    // Sets the current ability
    public void SetAbility(AbilityType nextAbility)
    {
        if (_currentAbility != AbilityType.NONE)
            _abilities[(int)_currentAbility].OnExit();

        ToggleAbilityArm(_currentAbility);

        _currentAbility = nextAbility;

        ToggleAbilityArm(_currentAbility);

        if (_currentAbility != AbilityType.NONE)
            _abilities[(int)_currentAbility].OnEnter();
    }

    // Activates the correct ability arm skin mesh renderer; disables if the next state is none
    public void ToggleAbilityArm(AbilityType nextAbility)
    {
        switch (nextAbility)
        {
            case AbilityType.NONE:
                abilityArms[_abilityArmIndex].enabled = false;
                break;

            case AbilityType.SICKLE:
                _abilityArmIndex = 0;
                abilityArms[_abilityArmIndex].enabled = true;
                break;

            case AbilityType.HAMMER:
                _abilityArmIndex = 1;
                abilityArms[_abilityArmIndex].enabled = true;
                break;

            case AbilityType.POT:
                abilityArms[_abilityArmIndex].enabled = true;
                _abilityArmIndex = 2;
                break;
        }
    }

    // Returns the closest parried enemy to the player
    public EnemyHandler GetClosestParriedEnemy()
    {
        // Clearing the map
        _sortedHitList.Clear();

        // Scan for enemies within radius of player
        Collider[] hits = Physics.OverlapSphere(transform.position, parriedCastRadius);

        // Populating a list with collided transforms and distances from origin
        foreach (Collider c in hits)
        {
            if (c.CompareTag("EnemySpecial"))
                _sortedHitList.Add(c);
        }

        // If we don't hit anything, print a warning then exit this function
        if (_sortedHitList.Count <= 0)
            return null;

        // Sorting list based on distance
        _sortedHitList.Sort((h1, h2) => Vector3.Distance(h1.transform.position, transform.position)
                             .CompareTo(Vector3.Distance(h2.transform.position, transform.position)));

        // Returning the closest enemy that is parried
        for (int i = 0; i < _sortedHitList.Count; ++i)
        {
            //EnemyHandler enemyHandler = _sortedHitList[i].transform.GetComponent<EnemyHandler>();
            EnemyHandler enemyHandler;
            AbsorbInteractable absorbInteractable;
            if (_sortedHitList[i].transform.TryGetComponent(out enemyHandler))
            {
                if (enemyHandler.IsParried())
                    return enemyHandler;
            }
            else if (_sortedHitList[i].transform.TryGetComponent(out absorbInteractable) && !_isAbosrbing)
            {
                if (absorbInteractable.IsAbsorbable())
                {
                    _isAbosrbing = true;
                    _animator.SetBool("Absorb", true);
                    _playerHandler.GetLocomotionHandler().Key_ActivateSlowdown();
                    absorbInteractable.Activate();
                }
                return null;
            }
        }

        // Returning null and logging an error, since we shouldn't get here
        return null;
    }
}
