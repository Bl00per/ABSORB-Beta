using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
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
    public SkinnedMeshRenderer playerWeapon;
    public SkinnedMeshRenderer playerWeapon2;
    public SkinnedMeshRenderer playerWeapon3;
    public Collider playerWeaponColl;
    public Collider playerWeaponColl2;
    public Collider playerWeaponColl3;
    public int playerWeaponDamage = 25;
    public int playerWeaponDamage2 = 50;
    public int playerWeaponDamage3 = 100;
    public AudioSource weaponSwingAudio;
    public ParticleSystem weaponPE;
    [Header("Debug purposes only [TURN-OFF/REMOVE IN BUILD]")]
    public bool debugDeath = false;
    [Header("Body")]
    public SkinnedMeshRenderer playerShader;
    private PlayerHandler _playerHandler;
    private SlowMotionManager _slowMoManager;
    private InputManager _inputManager;
    private Rigidbody _rigidbody;
    private Transform _transform;
    private Animator _animator;
    private Rigidbody _rb;
    private bool _justUsedMechanic = false;
    [Header("Enemy Attack Timers")]
    public float primaryAttackWindow = 1.0f;
    public float shieldAttackWindow = 0.5f;

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
        _rb = this.GetComponent<Rigidbody>();
        // Make sure the shield sphere is turned off by default
        shieldMeshRenderer.enabled = false;
        shieldState = ShieldState.Default;

        // Set temp timers
        //_tempShieldTimer = shieldTimer;
        _tempShieldCDTimer = shieldCooldown;

        enemy = null;
    }

    // Update is called once per frame
    private void Update()
    {
        UpdateShieldFSM();
        UpdateAttack();
        UpdateDeath();
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
    public GameObject lockOnGO;

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
                lockOnGO.transform.position = enemy.position;
                lockOnGO.SetActive(true);

                Vector3 direction = enemy.transform.position - _rb.transform.position;
                direction.y = 0;
                Quaternion rotation = Quaternion.LookRotation(direction);
                _rb.transform.rotation = Quaternion.Lerp(_rb.transform.rotation, rotation, attackRotSpeed * Time.deltaTime);
            }
            else // if an enemy is not in range
            {
                lockOnGO.SetActive(false);
            }
        }
        else // if the player is not attacking
        {
            lockOnGO.SetActive(false);
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
    }

    public void Key_DisableWeaponObject()
    {
        playerWeapon.enabled = false;
        playerWeaponColl.enabled = false;
        StartJustUsedMechanic(primaryAttackWindow);
    }

    public void Key_EnableMEDIUMPlayerWeaponObject()
    {
        playerWeapon2.enabled = true;
        playerWeaponColl2.enabled = true;
    }

    public void Key_DisableMEDIUMWeaponObject()
    {
        playerWeapon2.enabled = false;
        playerWeaponColl2.enabled = false;
        StartJustUsedMechanic(primaryAttackWindow);
    }

    public void Key_EnableBIGPlayerWeaponObject()
    {
        playerWeapon3.enabled = true;
        playerWeaponColl3.enabled = true;
    }

    public void Key_DisableBIGWeaponObject()
    {
        playerWeapon3.enabled = false;
        playerWeaponColl3.enabled = false;
        StartJustUsedMechanic(primaryAttackWindow);
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

    #endregion

    #region Take Damage

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("EnemyWeapon"))
        {
            EnemyHandler enemy = other.gameObject.GetComponent<EnemyWeapon>().GetEnemyHandler();
            if (shieldState != ShieldState.Shielding || enemy.GetEnemyType() == EnemyHandler.EnemyType.ELITE)
            {
                _playerHandler.TakeDamage(enemy.GetDamage());
                enemy.weaponCollider.enabled = false;
            }
        }
    }

    #endregion

    #region Shield
    // Attributes
    [Header("Timers", order = 0)]
    // [Range(0.1f, 5f)]
    // public float shieldTimer = 1.0f;
    [Range(0.1f, 5f)]
    public float shieldCooldown = 1.0f;
    private bool _canShield = true;

    // Properties
    //private float _tempShieldTimer;
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
        Debug.Log("Just used mechainc: " + _justUsedMechanic);

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
        if (_inputManager.GetShieldButtonPress() && _comboStart)
        {
            shieldState = ShieldState.Shielding;
            _animator.SetBool("Shield", true);
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

        StartJustUsedMechanic(shieldAttackWindow);
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

    // public float GetMaxShieldTimer()
    // {
    //     return _tempShieldTimer;
    // }

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
        if (!_playerHandler.GetIsAlive() && !_respawning)
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

    public IEnumerator Coroutine_JustUsedMechanic(float time)
    {
        _justUsedMechanic = true;
        yield return new WaitForSeconds(time);
        _justUsedMechanic = false;
    }
}
