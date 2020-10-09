using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityHammer : Ability
{
    [Header("References")]
    public GameObject hammerGameObject;
    public Transform groundSmashTransform;
    public Transform impactLocation;

    [Header("Properties")]
    public float groundSmashReparentTimer = 2.0f;
    public float areaOfEffect = 10.0f;

    private Vector3 _groundSmashPosition = Vector3.zero;
    private Animator _animator;
    private PlayerHandler _playerHandler;
    private Transform _groundSmashParent;
    private AudioSource _groundSmashAudio;
    private ParticleSystem _groundSmashParticleSystem;

    private void Awake()
    {
        _playerHandler = this.GetComponent<PlayerHandler>();
        _groundSmashParent = hammerGameObject.transform.parent;
        _groundSmashPosition = groundSmashTransform.position;
        _groundSmashAudio = groundSmashTransform.GetComponent<AudioSource>();
        _groundSmashParticleSystem = groundSmashTransform.GetComponent<ParticleSystem>();
    }

    private void Start()
    {
        _animator = _playerHandler.GetAnimator();
    }

    public override void OnEnter() { }

    public override void OnExit()
    {
        active = false;
        hammerGameObject.SetActive(true);
    }

    public override void Activate()
    {
        active = true;
        _animator.SetBool("Hammer", true);
    }

    // Key Event: Deactivates the hammer ability; only to be called through animation key event
    public void Key_DeactivateHammerAbility()
    {
        hammerGameObject.SetActive(false);
        _animator.SetBool("Hammer", false);
        abilityHandler.SetAbility(AbilityHandler.AbilityType.NONE);
    }

    // Key Event: Activates the ground smash VFX; only to be called through animation key event
    public void Key_ActivateHammerGroundSmash()
    {
        // Unparent and play VFX
        hammerGameObject.SetActive(true);
        groundSmashTransform.position = impactLocation.position;
        groundSmashTransform.rotation = impactLocation.rotation;
        groundSmashTransform.SetParent(null);
        _groundSmashParticleSystem.Play();
        _groundSmashAudio.Play();
        abilityHandler.abilityArms[(int)AbilityHandler.AbilityType.HAMMER].enabled = false;
        StartCoroutine(ReparentGroundSmash());

        // Check for any hits
        CheckForEnemyHit();
        active = false;
    }

    private IEnumerator ReparentGroundSmash()
    {
        yield return new WaitForSecondsRealtime(groundSmashReparentTimer);
        groundSmashTransform.SetParent(_groundSmashParent);
        groundSmashTransform.transform.localPosition = _groundSmashPosition;
        groundSmashTransform.transform.localRotation = Quaternion.identity;
    }

    private void CheckForEnemyHit()
    {
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, areaOfEffect, Vector3.up, 0.0f);
        foreach (RaycastHit hit in hits)
        {
            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                EnemyHandler enemyHit = hit.transform.gameObject.GetComponent<EnemyHandler>();
                float damageToTake = 0.0f;
                switch (enemyHit.GetEnemyType())
                {
                    case EnemyHandler.EnemyType.MINION:
                        damageToTake = damageToMinion;
                        break;

                    case EnemyHandler.EnemyType.SPECIAL:
                        damageToTake = damageToMinion;
                        break;

                    case EnemyHandler.EnemyType.ELITE:
                        damageToTake = damageToMinion;
                        break;
                }
                enemyHit.TakeDamage(damageToTake, AbilityHandler.AbilityType.HAMMER);
            }
            else if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Interactable") && hit.transform.gameObject.CompareTag("Rock"))
            {
                hit.transform.gameObject.SetActive(false);
            }
        }
    }
}
