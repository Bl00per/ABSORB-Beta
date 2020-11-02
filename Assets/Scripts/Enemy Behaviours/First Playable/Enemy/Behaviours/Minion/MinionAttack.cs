using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionAttack : AIBehaviour
{
    [Header("General Movement")]
    public float turnSpeed = 0.20f;

    [Header("References")]
    public GameObject weaponToEnable;

    private Vector3 _initialPos;
    private Quaternion _initialRot;
    private Animator _animator;
    private CombatHandler _combatHandler;

    private void Awake()
    {
        _animator = this.GetComponent<Animator>();
    }

    private void Start()
    {
        _initialPos = weaponToEnable.transform.localPosition;
        _initialRot = weaponToEnable.transform.localRotation;
        _combatHandler = this.brain.PlayerTransform.GetComponent<CombatHandler>();
    }

    public override void OnStateEnter()
    {
        _animator.SetBool("Attacking", true);
    }

    public override void OnStateExit() { }

    public override void OnStateFixedUpdate() { }

    public override void OnStateUpdate()
    {
        // Get direction to player 
        Vector3 dir = brain.GetDirectionToPlayer();

        // Rotate to face direction 
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), turnSpeed);
    }

    // Deactivates the collision check on the enemy's weapon 
    public void Key_DeactivateMinionAttack()
    {
        weaponToEnable.SetActive(false);
        _animator.SetBool("Attacking", false);
        brain.SetBehaviour("Movement");
    }

    public void Key_ActivateMinionAttack()
    {
        weaponToEnable.SetActive(true);
        weaponToEnable.transform.SetParent(null);
        StartCoroutine(ReparentWeapon());
    }

    private IEnumerator ReparentWeapon()
    {
        yield return new WaitForSeconds(2.0f);
        weaponToEnable.transform.SetParent(this.gameObject.transform);
        weaponToEnable.transform.localPosition = _initialPos;
        weaponToEnable.transform.localRotation = _initialRot;
    }
}
