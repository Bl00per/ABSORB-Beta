using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;
public class EnemyHandler : MonoBehaviour
{
    // Enemy type
    public enum EnemyType
    {
        MINION,
        SPECIAL,
        ELITE,
    }

    [Header("Damage FX")]
    public ParticleSystem damageEffect;
    public AudioSource damageEffectAudio;


    [Header("Parry FX")]
    public bool hasParryEffect = false;
    public ParticleSystem parryEffect;
    public AudioSource parryAudio;
    public float parryReparentTime = 1.0f;
    private Transform _parryParticleParent;

    [Header("Death FX")]
    public AudioSource deathSound;
    public float deathSoundLength = 2.0f;
    public ParticleSystem deathParticleEffect;

    [Header("Overall FX Properties")]
    public float overallFXTime = 1.0f;

    [Header("References")]
    public SkinnedMeshRenderer[] bodyMeshRenderer;
    public SkinnedMeshRenderer weaponMeshRenderer;
    public Collider weaponCollider;
    public AudioSource attackSFX;
    public ParticleSystem attackPE;

    [Header("Properties")]
    [SerializeField] EnemyType typeOfEnemy = EnemyType.MINION;
    [SerializeField] AbilityHandler.AbilityType typeOfAbility = AbilityHandler.AbilityType.NONE;
    public float maxHealth = 100.0f;
    public int baseDamage = 10;
    public float attackCooldown = 1.0f;
    private float _currentHealth = 0.0f;
    private bool _isAlive = true;


    [Header("Debug Options")]
    public bool printHealthStats = false;

    private SpecialParried _specialParried = null;
    private PlayerHandler _playerHandler;
    private EnemyGroupHandler _groupHandler = null;
    private AIBrain _aiBrain;
    private Rigidbody _rigidbody;
    private Collider _bodyCollider;
    private NavMeshAgent _navMeshAgent;
    private Animator _animator;
    private bool _isFunctional = true;
    private bool _justAttacked = false;
    private Vector3 _hitParryPosition = Vector3.zero;
    private Quaternion _hitParryRotation = Quaternion.identity;

    private void Awake()
    {
        // Setting the current health to be max
        _currentHealth = maxHealth;

        // Getting the functional components
        _aiBrain = this.GetComponent<AIBrain>();
        _rigidbody = this.GetComponent<Rigidbody>();
        _bodyCollider = this.GetComponent<Collider>();
        _navMeshAgent = this.GetComponent<NavMeshAgent>();
        _animator = this.GetComponent<Animator>();


        // Getting the special parried script if this enemy is a special
        if (typeOfEnemy == EnemyType.SPECIAL)
            _specialParried = this.GetComponent<SpecialParried>();

        // Get the particle parent
        if (hasParryEffect)
            _parryParticleParent = parryEffect.transform.parent;
    }

    private void Start()
    {
        // Getting the player handler component
        _playerHandler = _aiBrain.PlayerTransform.GetComponent<PlayerHandler>();
    }

    // Currently just destroying the enemy if the player attacks them
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("PlayerMelee") && other.gameObject.layer == LayerMask.NameToLayer("PlayerWeapon"))
            TakeDamage(_playerHandler.GetPrimaryAttackDamage());
    }

    // Makes the enemy take damage
    public void TakeDamage(float damage, AbilityHandler.AbilityType e_Ability = AbilityHandler.AbilityType.NONE)
    {
        // Subtracting the damage dealt by the player from the current health of this enemy
        _currentHealth -= damage;

        // If current health is less than 0
        if (_currentHealth <= 0)
        {
            // Set the enemy into this death state
            _isAlive = false;
            _aiBrain.SetBehaviour("Death");
        }

        // If the type of enemy isn't an elite or the player used the hammer ability; stagger this enemy
        if (typeOfEnemy != EnemyType.ELITE || e_Ability == AbilityHandler.AbilityType.HAMMER)
            _aiBrain.SetBehaviour("Stagger");

        // Playing damage VFX
        damageEffect.Play();
        damageEffectAudio.Play();

        // Printing debug
        if (printHealthStats)
            Debug.Log(gameObject.tag + " took damage. Current health: " + _currentHealth);
    }

    // Plays the hit parry effect
    public void PlayHitParryEffect()
    {
        if (hasParryEffect)
        {
            if (!parryEffect.gameObject.activeInHierarchy)
                parryEffect.gameObject.SetActive(true);

            parryEffect.transform.SetParent(null);
            parryEffect.Play();
            parryAudio.Play();
            StartCoroutine(ReparentHitEffect());
        }
    }

    private IEnumerator ReparentHitEffect()
    {
        yield return new WaitForSecondsRealtime(parryReparentTime);
        parryEffect.transform.SetParent(_parryParticleParent);
        parryEffect.transform.localPosition = _hitParryPosition;
        parryEffect.transform.localRotation = _hitParryRotation;
    }

    // Returns true if the entity is still alive
    public bool IsAlive()
    {
        return _isAlive;
    }

    // Returns the base damage of the enemy.
    public int GetDamage()
    {
        return baseDamage;
    }

    // Returns true if the enemy is in a parried state
    public bool IsParried()
    {
        // Safety check to make sure we only get the correct status
        if (typeOfEnemy == EnemyType.SPECIAL)
            return _specialParried.GetAbsorbable();
        else
            return false;
    }

    // Activates the weapons collider
    public void ActivateWeaponCollider()
    {
        weaponCollider.enabled = true;
    }

    // Deactivates the weapons collider
    public void DeactiveWeaponCollider()
    {
        weaponCollider.enabled = false;
    }

    public void key_PlayAttackVFX()
    {
        attackPE.Play();
    }
    public void key_PlayAttackSFX()
    {
        attackSFX.pitch = Random.Range(0.9f, 1.1f);
        attackSFX.PlayOneShot(attackSFX.clip);

    }

    // Returns the brain of this enemy
    internal AIBrain GetBrain()
    {
        return _aiBrain;
    }

    // Disables all functional components of the enemy; better than SetActive()
    public void SetFunctional(bool value)
    {
        if (value)
        {
            // Enabling all functional components
            _isAlive = true;
            _aiBrain.enabled = true;
            _rigidbody.isKinematic = false;
            foreach (SkinnedMeshRenderer mesh in bodyMeshRenderer)
            {
                mesh.enabled = true;
            }
            _bodyCollider.enabled = true;
            _animator.enabled = true;
            _navMeshAgent.enabled = true;
            weaponMeshRenderer.enabled = true;
            _isFunctional = true;
        }
        else
        {
            // Disabling all functional components
            _isAlive = false;
            _aiBrain.enabled = false;
            _rigidbody.isKinematic = true;
            foreach (SkinnedMeshRenderer mesh in bodyMeshRenderer)
            {
                mesh.enabled = false;
            }
            _bodyCollider.enabled = false;
            _animator.enabled = false;
            _navMeshAgent.enabled = false;
            weaponMeshRenderer.enabled = false;
            _isFunctional = false;
        }
    }

    // Kills the enemy
    public void Kill()
    {
        // Disabling functional components of enemy
        SetFunctional(false);

        // Playing death VFX
        PlayDeathFX();

        // Removes the group control over the enemy
        _groupHandler?.Remove(this);
    }

    private void PlayDeathFX()
    {
        // Playing death VFX
        deathParticleEffect.transform.SetParent(null);
        deathParticleEffect.Play();
        deathSound.Play();

        // Reparenting the VFX after N amount of time
        var cam = Camera.main.GetComponent<MonoBehaviour>();
        cam.StartCoroutine(ReparentDeathVFX());
    }

    public void Reset()
    {
        _aiBrain.SetBehaviour("Idle");
        _currentHealth = maxHealth;
        _isAlive = true;
    }

    private IEnumerator ReparentDeathVFX()
    {
        yield return new WaitForSeconds(overallFXTime);
        deathParticleEffect.transform.SetParent(this.gameObject.transform);
        deathParticleEffect.Stop();
        deathSound.Stop();
    }

    public IEnumerator Coroutine_JustAttacked()
    {
        yield return new WaitForSeconds(attackCooldown);
        _justAttacked = false;
    }

    public EnemyType GetEnemyType() => typeOfEnemy;

    public AbilityHandler.AbilityType GetAbilityType() => typeOfAbility;

    public Rigidbody GetRigidbody() => _rigidbody;

    public EnemyGroupHandler GetEnemyGroupHandler() => _groupHandler;

    public void SetEnemyGroupHandler(EnemyGroupHandler enemyGroupHandler) => _groupHandler = enemyGroupHandler;

    public PlayerHandler GetPlayerHandler() => _playerHandler;

    public bool GetFunctional() => _isFunctional;

    public bool GetJustAttacked() => _justAttacked;

    public void SetJustAttacked(bool value) => _justAttacked = value;
}