using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatHandler : MonoBehaviour
{
    /*
        This class will be used to control the "Combat" state machine within the 
        player controller.  
    */

    [Header("Shield")]
    public MeshRenderer shieldMeshRenderer;
    public Collider shieldSphereCollider;
    public AudioSource shieldSFX;
    [Header("Weapon")]
    public Renderer playerWeapon;
    public Renderer playerWeapon2;
    public Renderer playerWeapon3;
    public Collider playerWeaponColl;
    public Collider playerWeaponColl2;
    public Collider playerWeaponColl3;
    public int playerWeaponDamage = 25;
    public int playerWeaponDamage2 = 50;
    public int playerWeaponDamage3 = 100;
    public AudioSource weaponSwingAudio;
    public ParticleSystem weaponPE;
    public ParticleSystem weaponSummonPE1;
    public ParticleSystem weaponSummonPE2;
    public ParticleSystem weaponSummonPE3;
    [Header("Debug purposes only [TURN-OFF/REMOVE IN BUILD]")]
    public bool debugDeath = false;
    [Header("Body")]
    public SkinnedMeshRenderer playerShader;
    [Header("Enemy Attack Timers")]
    public float primaryAttackWindow = 1.0f;
    public float shieldAttackWindow = 0.5f;

    private PlayerHandler _playerHandler;
    private SlowMotionManager _slowMoManager;
    private InputManager _inputManager;
    private Rigidbody _rigidbody;
    private Transform _transform;
    private Animator _animator;
    private Rigidbody _rb;
    private AbilityHandler _abilityHandler;
    private CameraManager _cameraManager;
    private bool _justUsedMechanic = false;
    private bool _healingPlayer = false;
    private float _healthToHealTo = 0.0f;
    private float _healthLerpDuration = 1.0f;
    private Renderer[] _bodyRenderer;
    private float _localPlayerHP;

    // Start is called before first frame
    private void Start()
    {
        // Getting the required references
        _playerHandler = this.GetComponent<PlayerHandler>();
        _rigidbody = _playerHandler.GetRigidbody();
        _transform = _playerHandler.GetTransform();
        _animator = _playerHandler.GetAnimator();
        _inputManager = _playerHandler.GetInputManager();
        _slowMoManager = _playerHandler.GetSlowMotionManager();
        _abilityHandler = this.GetComponent<AbilityHandler>();
        _cameraManager = _playerHandler.GetCameraManager();
        _rb = this.GetComponent<Rigidbody>();
        _bodyRenderer = new Renderer[_abilityHandler.abidaroMesh.Length];
        _bodyRenderer = _abilityHandler.abidaroMesh;

        // Make sure the shield sphere is turned off by default
        shieldMeshRenderer.enabled = false;
        shieldState = ShieldState.Default;
        // Set temp timers
        _tempShieldCDTimer = shieldCooldown;

        // Set the local hp at that start = 100f
        _localPlayerHP = _playerHandler.GetCurrentHealth();

        enemy = null;
    }

    // Update is called once per frame
    private void Update()
    {
        UpdateShieldFSM();
        UpdateAttack();
        UpdateDeath();
        UpdatePlayerEmission();
        UpdateHeal();
    }

    #region Attacking 

    [Header("Attack Attributes")]
    public float minTimeBetweenAttack = 0.05f;
    public float maxTimeBetweenAttack = 1.0f;
    private float _attackTimer = 0.0f;
    public float attackRotSpeed;
    private bool _runAttackTimer = false;
    private bool _comboStart = true;
    private bool _attacking;
    private Transform enemy;
    //public GameObject lockOnGO;

    private void UpdateAttack()
    {
        EnemyDetector();
        if (_comboStart)
        {
            if (_inputManager.GetAttackButtonPress() && shieldState != ShieldState.Shielding && _comboStart)
            {
                _attacking = true;
                _animator.SetBool("Attack1", true);
                _animator.SetInteger("ComboNo.", 1);
                _comboStart = false;
                _runAttackTimer = true;
                ResetAttackTimer();
            }
        }

        if (_runAttackTimer)
        {
            _attackTimer += Time.deltaTime;
        }

        if (_attackTimer >= maxTimeBetweenAttack)
            AttackComboFinish();

    }

    public void EnemyDetector()
    {
        enemy = EnemyDetection.GetClosestEnemy(EnemyDetection.enemies, transform);

        if (_attacking) //if the player is attacking
        {
            if (enemy != null) //if an enemy is within range
            {
                // lockOnGO.transform.position = enemy.position;
                // lockOnGO.SetActive(true);

                Vector3 direction = enemy.transform.position - _rb.transform.position;
                direction.y = 0;
                Quaternion rotation = Quaternion.LookRotation(direction);
                _rb.transform.rotation = Quaternion.Lerp(_rb.transform.rotation, rotation, attackRotSpeed * Time.deltaTime);
            }
            else // if an enemy is not in range
            {
                // lockOnGO.SetActive(false);
            }
        }
        else // if the player is not attacking
        {
            //lockOnGO.SetActive(false);
        }
    }

    public float GetAttackTimer()
    {
        return _attackTimer;
    }

    public float ResetAttackTimer()
    {
        return _attackTimer = 0.0f;
    }

    public void AttackComboFinish()
    {
        _attacking = false;
        _comboStart = true;
        _animator.SetInteger("ComboNo.", 0);
        _animator.SetBool("Attack1", false);
        _animator.SetBool("Attack2", false);
        _animator.SetBool("Attack3", false);
        ResetAttackTimer();
    }

    public void Key_EnablePlayerWeaponObject()
    {
        playerWeapon.enabled = true;
        playerWeaponColl.enabled = true;
        StartJustUsedMechanic(primaryAttackWindow);
    }

    public void Key_DisableWeaponObject()
    {
        playerWeapon.enabled = false;
        playerWeaponColl.enabled = false;
    }

    public void Key_EnableMEDIUMPlayerWeaponObject()
    {
        playerWeapon2.enabled = true;
        playerWeaponColl2.enabled = true;
        StartJustUsedMechanic(primaryAttackWindow);
    }

    public void Key_DisableMEDIUMWeaponObject()
    {
        playerWeapon2.enabled = false;
        playerWeaponColl2.enabled = false;
    }

    public void Key_EnableBIGPlayerWeaponObject()
    {
        playerWeapon3.enabled = true;
        playerWeaponColl3.enabled = true;
        StartJustUsedMechanic(primaryAttackWindow);
    }

    public void Key_DisableBIGWeaponObject()
    {
        playerWeapon3.enabled = false;
        playerWeaponColl3.enabled = false;
    }

    public void Key_PlayWeaponSound()
    {
        if (weaponSwingAudio != null)
            weaponSwingAudio.Play();
    }
    public void Key_PlayWeaponPE()
    {
        if (weaponPE != null)
            weaponPE.Play();
    }
    public void Key_PlayWeaponSummonPE1()
    {
        if (weaponSummonPE1 != null)
            weaponSummonPE1.Play();
    }
    public void Key_PlayWeaponSummonPE2()
    {
        if (weaponSummonPE2 != null)
            weaponSummonPE2.Play();
    }
    public void Key_PlayWeaponSummonPE3()
    {
        if (weaponSummonPE3 != null)
            weaponSummonPE3.Play();
    }

    public void Key_SetAttack1Bool()
    {
        _animator.SetBool("Attack1", false);
    }

    public void Key_SetAttack2Bool()
    {
        _animator.SetBool("Attack2", false);
    }

    public void Key_SetAttack3Bool()
    {
        _animator.SetBool("Attack3", false);
    }

    public void key_ShieldFX()
    {
        shieldSFX.Play();
    }

    public void key_SmallVibrate()
    {
        StartCoroutine(ControllorVibration.Vibrate(.2f, .2f, .2f));
    }
    public void key_LargeVibrate()
    {
        StartCoroutine(ControllorVibration.Vibrate(1f, 1f, .4f));
    }

    #endregion

    #region Health Modifiers

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("EnemyWeapon") || other.gameObject.CompareTag("EnemyProjectile"))
        {
            // Get enemy handler out of the enemy weapon
            EnemyHandler enemy = other.gameObject.GetComponent<EnemyWeapon>().GetEnemyHandler();

            // // If shield state default but the components are enabled, then we exit this function
            // if (shieldState == ShieldState.Default && (shieldSphereCollider.enabled || shieldMeshRenderer.enabled))
            //     return;

            if (shieldState != ShieldState.Shielding || enemy.GetEnemyType() == EnemyHandler.EnemyType.ELITE)
            {
                _playerHandler.TakeDamage(enemy.GetDamage());
                //enemy.weaponCollider.enabled = false;
                StartCoroutine(ControllorVibration.Vibrate(.5f, .5f, .1f));
            }
        }
    }

    // Check for when the player health decreases
    private bool PlayerHealthUpdated()
    {
        if (_localPlayerHP != _playerHandler.GetCurrentHealth())
        {
            _localPlayerHP = _playerHandler.GetCurrentHealth();
            return true;
        }
        else
            return false;
    }

    // Update the players emission and the vignette as they take damage or heal
    private void UpdatePlayerEmission()
    {
        // Lower the emission intensity when the player takes damage
        if (!_abilityHandler.GetColorChange() && PlayerHealthUpdated())
        {
            for (int i = 0; i < _bodyRenderer.Length; i++)
            {
                _bodyRenderer[i].material.SetColor("_EmissionColor", _abilityHandler.GetCurrentColor() *
                ((_abilityHandler.abilityIntensity / _playerHandler.maxHealth) * _playerHandler.GetCurrentHealth()));
            }
            //Debug.Log("Emission decreased");
        }

        float temp = (-_playerHandler.GetCurrentHealth() / _playerHandler.maxHealth) + 1f;
        // When you gain max health again, makes it so isn't doesn't read temp when it's negative
        if (temp <= 0.1f)
            _cameraManager.SetVignetteIntensity(0.1f);
        else
            _cameraManager.SetVignetteIntensity(temp);
    }

    // Adds value onto health overtime
    public void HealOvertime(float value, float duration)
    {
        _healthLerpDuration = duration;
        _healthToHealTo = 0.0f;
        _healthToHealTo += _playerHandler.GetCurrentHealth() + value; //60
        _healingPlayer = true;
    }

    // Lerps the health from current to the new health
    private void UpdateHeal()
    {
        // Only heal the player if the HealOvertime(f) function has been set
        if (_healingPlayer)
        {
            // Grabbing the health from the player
            float health = _playerHandler.GetCurrentHealth();

            // Linearly interpolating the current health to the "health to heal to" property
            health = Mathf.Lerp(health, _healthToHealTo, 0.5F / _healthLerpDuration);

            // Checking if health is above out maximum health before setting it to the player
            if (health > _healthToHealTo - 0.1F)
            {
                health = _healthToHealTo;
                _healingPlayer = false;
            }

            // Checking if health is above out maximum health before setting it to the player
            if (health > _playerHandler.GetMaxHealth() - 0.1F)
            {
                health = _playerHandler.GetMaxHealth();
                _healingPlayer = false;
            }

            // Setting player health to calculated health
            _playerHandler.SetCurrentHealth(health);
        }
    }

    #endregion

    #region Shield
    // Attributes
    [Header("Timers", order = 0)]
    [Range(0.1f, 5f)]
    public float shieldCooldown = 1.0f;
    private bool _canShield = true;

    // Properties
    private float _tempShieldCDTimer;

    public enum ShieldState
    {
        Default,    // Shield can be used & Checking for input
        Shielding,  // Player is currently shielded
        Cooldown    // Shield is currently on cooldown
    }
    [HideInInspector]
    public ShieldState shieldState;

    // Updates the shields FSM
    private void UpdateShieldFSM()
    {
        switch (shieldState)
        {
            case ShieldState.Default:
                EnableShield();
                break;
            case ShieldState.Shielding:
                Shielding();
                break;
            case ShieldState.Cooldown:
                Cooldown();
                break;
        }
    }

    private void EnableShield()
    {
        // When the player hits the shield key
        if (_inputManager.GetShieldButtonPress()) //&& _comboStart)
        {
            shieldState = ShieldState.Shielding;
            _animator.SetBool("Shield", true);
            StartJustUsedMechanic(shieldAttackWindow);
        }
    }

    private void Shielding()
    {
        // This is only here just to check the bool so it can move onto cooldown
        if (!_canShield)
            shieldState = ShieldState.Cooldown;
    }

    private void Cooldown()
    {
        shieldCooldown -= Time.deltaTime;
        _animator.SetBool("Shield", false);

        //StartJustUsedMechanic(shieldAttackWindow);
        if (shieldCooldown <= 0)
        {
            shieldCooldown = _tempShieldCDTimer;
            shieldState = ShieldState.Default;
            _canShield = true;

        }
    }

    public void StartJustUsedMechanic(float time)
    {
        if (!_justUsedMechanic)
        {
            StartCoroutine(Coroutine_JustUsedMechanic(time));
        }
    }

    public bool SetCanShield(bool value)
    {
        return _canShield = value;
    }

    public bool GetCanShield()
    {
        return _canShield;
    }

    #endregion

    #region SlowMotion

    public void Key_ActivateSlowMotion()
    {
        _slowMoManager.ActivateSlowMotion();
    }

    public void Key_DeactivateSlowMotion()
    {
        _slowMoManager.DeactivateSlowMotion();
    }

    #endregion

    #region Death
    [Header("Death Attributes")]
    public AudioSource deathSFX1;
    public AudioSource deathSFX2;
    public ParticleSystem deathPE;
    public float timeTillRespawn = 3.0f;
    private bool _respawning = false;

    private void UpdateDeath()
    {
        // Make the player fade away as they take damage
        playerShader.material.SetFloat("_AlphaClip", _playerHandler.GetCurrentHealth());
        // Temp to force the player dead
        if (Input.GetKeyDown(KeyCode.Backspace) && debugDeath)
        {
            _playerHandler.SetIsAlive(false);
            _animator.SetBool("Death", true);
            // Debug.Log("Player dead");
        }

        // Update if the player is alive or not
        if (_playerHandler.GetCurrentHealth() <= 0)
        {
            _playerHandler.SetIsAlive(false);
            _animator.SetBool("Death", true);
        }

        // Is the player is dead
        if (!_playerHandler.GetIsAlive() && !_respawning && _playerHandler.GetCurrentHealth() <= 0)
        {
            _respawning = true;
            _inputManager.DisableInput();
            _playerHandler.GetCameraManager().DisableCameraMovement(); // TEMPORARY FOR NOW
            StartCoroutine(Respawn());
        }
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(timeTillRespawn);
        // Go to last checkpoint position
        _playerHandler.RespawnPlayer();
        _respawning = false;
        // Go to main menu
        //UnityEngine.SceneManagement.SceneManager.LoadScene("Main_Menu");
    }

    public void key_deathSFX1()
    {
        deathSFX1.Play();
    }

    public void key_deathSFX2()
    {
        deathSFX2.Play();
    }
    public void key_deathPE()
    {
        deathPE.Play();
    }

    #endregion

    // Function I found online that checks if an animation is currently playing
    // Figured it might be handy sometime
    bool isPlaying(Animator anim, string stateName)
    {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName(stateName) &&
                anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
            return true;
        else
            return false;
    }

    public bool GetJustUsedMechanic()
    {
        return _justUsedMechanic;
    }

    public bool GetIsHealing()
    {
        return _healingPlayer;
    }

    public IEnumerator Coroutine_JustUsedMechanic(float time)
    {
        _justUsedMechanic = true;
        yield return new WaitForSeconds(time);
        _justUsedMechanic = false;
    }
}
