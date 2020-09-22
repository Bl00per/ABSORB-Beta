using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Absorb : MonoBehaviour
{
    // /*
    //  * TODO:
    //  * Slowdown player. (Reference Will's script)
    //  * Make the player face the enemy they are absorbing.
    //  * Play the abosrb animations on both enemy and player.
    //  * Set players ability.
    //  * Kill enemy.
    //  */


    // // The enemy which we will abosrb
    // private AIBrain _targetEnemy = null;
    // public AIBrain TargetEnemy
    // {
    //     get { return _targetEnemy;  }
    //     set { _targetEnemy = value; }
    // }

    // [Header("Properties")]
    // public float turnSpeed = 0.5F;
    // public float animationTime = 2.0f;
    // private bool _isAbosrbing = false;
    // private PlayerSlowdown playerSlowdown; 
    // private InputManager _inputManager;
    // private Animator _animator;
    // private AbilityManager _abilityManager;

    // private void Awake()
    // {
    //     playerSlowdown = this.GetComponent<PlayerSlowdown>();
    //     _inputManager = FindObjectOfType<InputManager>();
    //     _animator = this.GetComponent<Animator>();
    //     _abilityManager = this.GetComponent<AbilityManager>();
    // }

    // // Called every frame
    // private void Update()
    // {
    //     if (_abilityManager.IsActive() || !_targetEnemy)
    //         return;

    //     // Check if we should start abosrbing
    //     if (_inputManager.GetSpecialAttackButtonPress() && !_isAbosrbing)
    //     {
    //         ActivateAbsorb();
    //         StartCoroutine(WaitFor(animationTime));
    //     }
    // }

    // // Returns true after n amount of seconds
    // private IEnumerator WaitFor(float seconds)
    // {
    //     yield return new WaitForSecondsRealtime(seconds);
    //     DeactivateAbsorb();
    // }

    // public void ActivateAbsorb()
    // {
    //     _targetEnemy.SetBehaviour("Absorbed");
    //     playerSlowdown.SetSlowdown();
    //     _animator.SetBool("AbsorbPose", true);
    //     _isAbosrbing = true;
    //     _targetEnemy = null;
    // }

    // public void DeactivateAbsorb()
    // {
    //     playerSlowdown.SetSpeedUp();
    //     _animator.SetBool("AbsorbPose", false);
    //     _isAbosrbing = false;
    //     _targetEnemy = null;
    // }

    // public bool IsActive()
    // {
    //     return _isAbosrbing;
    // }
}