using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attacking : MonoBehaviour
{

    [Header("References")]
    public GameObject playerWeapon;
    public AudioSource weaponSwingAudio;


    public Animator playerAnimator;
    [Range(0f, 3f)]
    public float animationSpeedMultiplier = 1.0f;

    private SpecialParryBlock _specialParry;
    private InputManager _inputManager;

    void Start()
    {
        _specialParry = GetComponent<SpecialParryBlock>();
        _inputManager = FindObjectOfType<InputManager>();
    }

    // Update is called once per frame
    void Update()
    {
        playerAnimator.speed = animationSpeedMultiplier;

        // ***Player is able to input 2 attack triggers in quick succession instead of 1 at a time***
        // Needs more research and tinkering if its going to work
        // Check if the animation is currently in play
        //if (!(playerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1) && !playerAnimator.IsInTransition(0))
        //{
        //animationFinished = true;
        if (_inputManager.GetAttackButtonPress() && _specialParry.shieldState != SpecialParryBlock.ShieldState.Shielding)
            playerAnimator.SetTrigger("playAttack");
        //}

    }

    public void EnablePlayerWeaponObject()
    {
        playerWeapon.SetActive(true);
    }

    public void DisableWeaponObject()
    {
        playerWeapon.SetActive(false);
    }

    public void PlayWeaponSound()
    {
        weaponSwingAudio.Play();
    }
}
