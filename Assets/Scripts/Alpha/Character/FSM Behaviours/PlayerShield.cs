using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShield : StateMachineBehaviour
{
    PlayerHandler _playerHandler;
    void Awake()
    {
        _playerHandler = FindObjectOfType<PlayerHandler>();
    }

    //OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Setting the current state within the player handler
        _playerHandler.SetState(PlayerHandler.PlayerAnimatorState.SHIELD);

        // Setting the animator bool false so this state duration equals the animation time
        animator.SetBool("Shield", false);

        if (_playerHandler != null)
        {
            _playerHandler.GetCombatHandler().shieldMeshRenderer.enabled = true;
            _playerHandler.GetCombatHandler().shieldSphereCollider.enabled = true;
        }
        else
            Debug.LogWarning("Player Handler not found.");
    }

    //OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    // override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    // {
    // }

    //OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //         _animator.SetBool("Defence", false);
        if (_playerHandler != null)
        {
            _playerHandler.GetCombatHandler().shieldMeshRenderer.enabled = false;
            _playerHandler.GetCombatHandler().shieldSphereCollider.enabled = false;
            _playerHandler.GetCombatHandler().SetCanShield(false);
        }       
        else
            Debug.LogWarning("Player Handler not found.");
    }

    // //OnStateMove is called right after Animator.OnAnimatorMove()
    // override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    // {
    //     // Implement code that processes and affects root motion
    // }

    // //OnStateIK is called right after Animator.OnAnimatorIK()
    // override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    // {
    //    // Implement code that sets up animation IK (inverse kinematics)
    // }
}
