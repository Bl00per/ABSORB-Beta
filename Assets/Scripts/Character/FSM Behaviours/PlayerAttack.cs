using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : StateMachineBehaviour
{
    public string parameterName;
    public int value;

    private PlayerHandler _playerHandler;
    private CombatHandler _combatHandler;
    private InputManager _inputManager;

    void Awake()
    {
        _playerHandler = FindObjectOfType<PlayerHandler>();
    }

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _combatHandler = _playerHandler.GetCombatHandler();
        _inputManager = _playerHandler.GetInputManager();
        _playerHandler.SetPrimaryAttackDamage(_playerHandler.GetCombatHandler().playerWeaponDamage);
        // Setting the current state within the player handler
        _playerHandler.SetState(PlayerHandler.PlayerAnimatorState.ATTACK);
    }

    //OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Check for: input, not shielding, attack not getting spammed
        if (_inputManager.GetAttackButtonPress() && _combatHandler.shieldState != CombatHandler.ShieldState.Shielding && _combatHandler.GetAttackTimer() >= _combatHandler.minTimeBetweenAttack)
        {
            _combatHandler.ResetAttackTimer();
            animator.SetInteger(parameterName, value);
            animator.SetBool("Attack2", true);
        }
    }
}
