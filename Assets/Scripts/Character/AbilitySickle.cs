using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilitySickle : Ability
{
    [Header("Properties")]
    public float vfxTime = 1.5f;

    [Header("Refereneces")]
    public GameObject sickleGameObject;
    public Collider sickleCollider;
    public AudioSource sickleSFX;

    private PlayerHandler _playerHandler;
    private Animator _animator;
    private Transform _parent;
    private Vector3 _initPos = Vector3.zero;
    private Quaternion _initRot = Quaternion.identity;

    public void Awake()
    {
        _typeOfAbility = AbilityHandler.AbilityType.SICKLE;
        _playerHandler = this.GetComponent<PlayerHandler>();
        _parent = sickleGameObject.transform.parent;
    }

    public void Start()
    {
        _animator = _playerHandler.GetAnimator();
        _initPos = sickleGameObject.transform.localPosition;
        _initRot = sickleGameObject.transform.localRotation;
    }

    public override void OnEnter() { }

    public override void OnExit() { }

    public override void Activate()
    {
        _animator.SetBool("Sickle", true);
    }

    public void Key_ActivateSickleAbility()
    {
        sickleCollider.enabled = true;
        sickleGameObject.SetActive(true);
    }

    public void Key_DeactivateSickleAbility()
    {
        sickleCollider.enabled = false;
        _animator.SetBool("Sickle", false);
        abilityHandler.SetAbility(AbilityHandler.AbilityType.NONE);
        StartCoroutine(ReparentVFX());
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

    private IEnumerator ReparentVFX()
    {
        sickleGameObject.transform.SetParent(null);
        yield return new WaitForSeconds(vfxTime);
        sickleGameObject.transform.SetParent(_parent);
        sickleGameObject.transform.localPosition = _initPos;
        sickleGameObject.transform.localRotation = _initRot;
        sickleGameObject.SetActive(false);
    }
}