using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    [Header("Properties")]
    [SerializeField] EnemyType typeOfEnemy = EnemyType.MINION;
    [SerializeField] AbilityHandler.AbilityType typeOfAbility = AbilityHandler.AbilityType.NONE;

    public float maxHealth = 100.0f;
    public int baseDamage = 10;

    [Header("Debug Options")]
    public bool printHealthStats = false;

    private float _currentHealth = 0.0f;
    private bool _isAlive = true;
    private SpecialParried _specialParried = null;
    private PlayerHandler _playerHandler;
    private EnemyGroupHandler _groupHandler = null;

    // The collider of this enemies weapon
    public Collider weaponCollider;

    // The brain of this enemy
    private AIBrain _aiBrain;

    // The rigidbody attached to this object
    private Rigidbody _rigidbody;

    // Reference to the spawner which created this enemy
    private Spawner _spawner;

    private void Awake()
    {
        // Setting the current health to be max
        _currentHealth = maxHealth;

        // Getting the components
        _aiBrain = this.GetComponent<AIBrain>();
        _rigidbody = this.GetComponent<Rigidbody>();

        // Getting the special parried script if this enemy is a special
        if (typeOfEnemy == EnemyType.SPECIAL)
            _specialParried = this.GetComponent<SpecialParried>();

        // Get the particle parent
        _parryParticleParent = parryEffect.transform.parent;
    }

    private void Start()
    {
        _playerHandler = _aiBrain.PlayerTransform.GetComponent<PlayerHandler>();
    }

    private void Update()
    {
        UpdateSlowMo();

        // Checking if the enemy is still alive
        if (!_isAlive)
            _aiBrain.SetBehaviour("Death");
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
        _currentHealth -= damage;

        if (_currentHealth <= 0)
            _isAlive = false;

        if (typeOfEnemy != EnemyType.ELITE)
            _aiBrain.SetBehaviour("Stagger");

        else if (e_Ability == AbilityHandler.AbilityType.HAMMER)
            _aiBrain.SetBehaviour("Stagger");

        damageEffect.Play();
        damageEffectAudio.Play();

        if (printHealthStats)
            Debug.Log(gameObject.tag + " took damage. Current health: " + _currentHealth);
    }

    // Plays the hit parry effect
    public void PlayHitParryEffect()
    {
        if (hasParryEffect)
        {
            parryEffect.transform.SetParent(null);
            parryEffect.Play();
            parryAudio.Play();
            StartCoroutine(Slow());
            StartCoroutine(ReparentHitEffect());
        }
    }

    private IEnumerator ReparentHitEffect()
    {
        yield return new WaitForSecondsRealtime(parryReparentTime);
        parryEffect.transform.SetParent(_parryParticleParent);
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

    // Sets up the spawner
    public void SetupSpawner(Spawner spawner)
    {
        _spawner = spawner;
    }

    // Returns the brain of this enemy
    internal AIBrain GetBrain()
    {
        return _aiBrain;
    }

    // Returns the spawner
    public Spawner GetSpawner()
    {
        return _spawner;
    }

    // Kills the enemy
    public void Kill()
    {
        // Playing death VFX
        PlayDeathFX();

        _spawner?.RemoveEnemy(this.gameObject);

        // Removes enemy from group, if within one
        _groupHandler?.Remove(this);

        // Resetting all stats and adding the enemy back into the object pool
        ResetAndAddToQueue();
    }

    private void PlayDeathFX()
    {
        // Playing death VFX
        deathParticleEffect.transform.SetParent(null);
        deathParticleEffect.Play();
        deathSound.Play();

        // Reparenting the VFX after N amount of time
        var cam = Camera.main.GetComponent<MonoBehaviour>();
        cam.StartCoroutine(ReparentVFX());
    }

    public void ResetAndAddToQueue()
    {

        _currentHealth = maxHealth;
        _isAlive = true;

        deathParticleEffect.transform.SetParent(this.gameObject.transform);
        deathParticleEffect.Stop();
        deathSound.Stop();

        gameObject.SetActive(false);
        _isAlive = true;
        _aiBrain.SetBehaviour("Idle");


        //ObjectPooler.Instance.poolDictionary[gameObject.tag].Enqueue(gameObject);
        gameObject.transform.SetParent(null); //quick error fix
    }

    private IEnumerator ReparentVFX()
    {
        yield return new WaitForSecondsRealtime(overallFXTime);
        deathParticleEffect.transform.SetParent(this.gameObject.transform);
        deathParticleEffect.Stop();
        deathSound.Stop();
    }

    public EnemyType GetEnemyType()
    {
        return typeOfEnemy;
    }

    public AbilityHandler.AbilityType GetAbilityType()
    {
        return typeOfAbility;
    }

    public Rigidbody GetRigidbody()
    {
        return _rigidbody;
    }

    public EnemyGroupHandler GetEnemyGroupHandler()
    {
        return _groupHandler;
    }

    public void SetEnemyGroupHandler(EnemyGroupHandler enemyGroupHandler)
    {
        _groupHandler = enemyGroupHandler;
    }

    public PlayerHandler GetPlayerHandler()
    {
        return _playerHandler;
    }

    #region SlowMotion

    [Header("Slow Motion Attributes")]
    [Range(1, 100)]
    public int slowMotionPercentage = 50;
    public AnimationCurve tweenEase;
    private float tempSlowMoPercentage = 0.0f;
    private float currentTimeScale;
    private float defaultTimeScale = 1.0f;
    public float slowMoTime;
    private bool activateSlowmo = false;

    private void UpdateSlowMo()
    {
        tempSlowMoPercentage = slowMotionPercentage / 100.0f;

        if (activateSlowmo)
        {
            currentTimeScale = Mathf.Lerp(currentTimeScale, tempSlowMoPercentage, tweenEase.Evaluate(Time.time));
            Time.timeScale = currentTimeScale;
        }
        else if (!activateSlowmo && currentTimeScale < defaultTimeScale)
        {
            currentTimeScale = Mathf.Lerp(currentTimeScale, defaultTimeScale, 0.2f);
            Time.timeScale = currentTimeScale;
        }
    }
    
    private IEnumerator Slow()
    {
        activateSlowmo = true;

        yield return new WaitForSeconds(slowMoTime);

        activateSlowmo = false;
    }

    #endregion
}