using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilitySickle : Ability
{
    [Header("Refereneces")]
    public GameObject sickleGameObject;
    private Collider sickleCollider;
    private PlayerHandler _playerHandler;
    private Animator _animator;

    public void Awake()
    {
        _playerHandler = this.GetComponent<PlayerHandler>();
    }

    public void Start()
    {
        _animator = _playerHandler.GetAnimator();
        sickleCollider = sickleGameObject.GetComponent<Collider>();
    }

    public override void OnEnter() { }

    public override void OnExit() { }

    public override void Activate()
    {
        _animator.SetBool("Sickle", true);
        sickleGameObject.SetActive(true);
    }
    public void Key_ActivateSickleCollider()
    {
        sickleCollider.enabled = true;
    }
    public void Key_DeactivateSickleCollider()
    {
        sickleCollider.enabled = false;
    }

    public void Key_DeactivateSickleAbility()
    {
        _animator.SetBool("Sickle", false);
        sickleGameObject.SetActive(false);
        abilityHandler.SetAbility(AbilityHandler.AbilityType.NONE);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.gameObject.layer == LayerMask.NameToLayer("Interactable") && other.transform.gameObject.CompareTag("Vines"))
        {
            //Debug.Log("Hit interactable");
            other.transform.gameObject.SetActive(false);
        }
    }
    
        
}

