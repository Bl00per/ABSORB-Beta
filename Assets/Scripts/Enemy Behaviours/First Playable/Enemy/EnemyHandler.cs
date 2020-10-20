using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
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
    
    [Header("Revive FX")]
    public AudioSource reviveSound;
    public ParticleSystem reviveVFX;

    [Header("Overall FX Properties")]
    public float overallFXTime = 1.0f;

    [Header("References")]
    public Renderer[] bodyMeshRenderer;
    public Renderer weaponMeshRenderer;
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
    private SlowMotionManager _slowMoManager;
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
        _slowMoManager = _playerHandler.GetSlowMotionManager();

        currentTimeScale = Time.timeScale;
    }

    public void Update()
    {
        UpdateSlowMo();
    }

    // Currently just destroying the enemy if the player attacks them
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerWeapon"))
        {
            if (other.gameObject.CompareTag("PlayerMelee") && typeOfEnemy != EnemyType.ELITE)
            {
                TakeDamage(_playerHandler.GetPrimaryAttackDamage());
            }
            else if (other.gameObject.CompareTag("Ability"))
            {
                Ability usedAbility = null;
                if (other.transform.TryGetComponent(out usedAbility))
                {
                    switch (typeOfEnemy)
                    {
                        case EnemyType.MINION:
                            TakeDamage(usedAbility.damageToMinion, usedAbility.GetAbilityType());
                            break;

                        case EnemyType.SPECIAL:
                            TakeDamage(usedAbility.damageToSpecial, usedAbility.GetAbilityType());
                            break;

                        case EnemyType.ELITE:
                            TakeDamage(usedAbility.damageToElite, usedAbility.GetAbilityType());
                            break;
                    }
                }
            }
        }
    }

    // Makes the enemy take damage
    public void TakeDamage(float damage, AbilityHandler.AbilityType eAbility = AbilityHandler.AbilityType.NONE)
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
        if (typeOfEnemy != EnemyType.ELITE || eAbility != AbilityHandler.AbilityType.NONE)
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
        if (weaponCollider != null)
            weaponCollider.enabled = true;
    }

    // Deactivates the weapons collider
    public void DeactiveWeaponCollider()
    {
        if (weaponCollider != null)
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
            if (reviveVFX != null && reviveSound != null)
            {
                reviveSound.Play();
                reviveVFX.Play();
            }
                
            _isAlive = true;
            _aiBrain.enabled = true;
            _rigidbody.isKinematic = false;
            foreach (Renderer mesh in bodyMeshRenderer)
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
            foreach (Renderer mesh in bodyMeshRenderer)
            {
                mesh.enabled = false;
            }
            _bodyCollider.enabled = false;
            _animator.enabled = false;
            _navMeshAgent.enabled = false;
            weaponMeshRenderer.enabled = false;
            weaponCollider.enabled = false;
            _isFunctional = false;
        }

        //Debug.Log($"Set functional: {value} -> On enemy: {gameObject.name}");
    }

    // Kills the enemy
    public void Kill()
    {
        // // Disabling functional components of enemy
        // SetFunctional(false);

        // Playing death VFX
        PlayDeathFX();
        EnemyDetection.enemies.Clear();
        // Removes the group control over the enemy
        if (_groupHandler != null)
            _groupHandler.Remove(this); // Group handler handles the SetFunctionl call
        else
            SetFunctional(false);
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

    #region SlowMotion

    [Header("Slow Motion Attributes")]
    [Range(1, 100)]
    public int slowMotionPercentage = 50;
    public AnimationCurve tweenEase;
    private float tempSlowMoPercentage = 0.0f;
    private float currentTimeScale;
    private float defaultTimeScale = 1.0f;
    private bool activateSlowmo = false;
    public AudioMixer _mixer;

    public void UpdateSlowMo()
    {
        tempSlowMoPercentage = slowMotionPercentage / 100.0f;

        if (activateSlowmo)
        {
            _mixer.SetFloat("MasterPitch", 0.5f);
            currentTimeScale = Mathf.Lerp(currentTimeScale, tempSlowMoPercentage, tweenEase.Evaluate(Time.time));
            Time.timeScale = currentTimeScale;
        }
        else if (!activateSlowmo && currentTimeScale < defaultTimeScale)
        {
            _mixer.SetFloat("MasterPitch", 1);
            //Debug.Log(_mixer.GetFloat("MasterPitch",))
            currentTimeScale = Mathf.Lerp(currentTimeScale, defaultTimeScale, 0.2f);
            Time.timeScale = currentTimeScale;
        }
    }

    public void Key_ActivateSlowMotion()
    {
        _slowMoManager.ActivateSlowMotion();
    }

    public void Key_DeactivateSlowMotion()
    {
        _slowMoManager.DeactivateSlowMotion();
    }

    #endregion

    public EnemyType GetEnemyType() => typeOfEnemy;

    public AbilityHandler.AbilityType GetAbilityType() => typeOfAbility;

    public Rigidbody GetRigidbody() => _rigidbody;

    public Animator GetAnimator() => _animator;

    public EnemyGroupHandler GetEnemyGroupHandler() => _groupHandler;

    public void SetEnemyGroupHandler(EnemyGroupHandler enemyGroupHandler) => _groupHandler = enemyGroupHandler;

    public PlayerHandler GetPlayerHandler() => _playerHandler;

    public bool GetFunctional() => _isFunctional;

    public bool GetJustAttacked() => _justAttacked;

    public void SetJustAttacked(bool value) => _justAttacked = value;
}