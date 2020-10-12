using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilitySickle : Ability
{
    [Header("Refereneces")]
    public GameObject sickleGameObject;
    public Collider sickleCollider;
    public AudioSource sickleSFX;

    private PlayerHandler _playerHandler;
    private Animator _animator;

    public void Awake()
    {
        _playerHandler = this.GetComponent<PlayerHandler>();
    }

    public void Start()
    {
        _animator = _playerHandler.GetAnimator();
    }

    public override void OnEnter() { }

    public override void OnExit() { }

    public override void Activate()
    {
        _animator.SetBool("Sickle", true);
        //sickleGameObject.SetActive(true);
    }
    public void Key_ActivateSickleAbility()
    {

        sickleGameObject.SetActive(true);

    }
    public void Key_DeactivateSickleAbility()
    {
        _animator.SetBool("Sickle", false);
        sickleGameObject.SetActive(false);
        abilityHandler.SetAbility(AbilityHandler.AbilityType.NONE);
    }
    public void Key_sickleSFX()
    {
         sickleSFX.Play();
    }

    public void Key_SetSickleCollider()
    {
        sickleCollider.enabled = true;
    }
    
    public void Key_UnsetSickleCollider()
    {
        sickleCollider.enabled = false;
    }


}
