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
    [Header("Body")]
    public SkinnedMeshRenderer playerShader;
    private PlayerHandler _playerHandler;
    private InputManager _inputManager;
    private Rigidbody _rigidbody;
    private Transform _transform;
    private Animator _animator;

    // Start is called before first frame
    private void Start()
    {
        // Getting the required references
        _playerHandler = this.GetComponent<PlayerHandler>();
        _rigidbody = _playerHandler.GetRigidbody();
        _transform = _playerHandler.GetTransform();
        _animator = _playerHandler.GetAnimator();
        _inputManager = _playerHandler.GetInputManager();

        // Make sure the shield sphere is turned off by default
        shieldMeshRenderer.enabled = false;
        shieldState = ShieldState.Default;

        // Set temp timers
        //_tempShieldTimer = shieldTimer;
        _tempShieldCDTimer = shieldCooldown;

        currentTimeScale = Time.timeScale;
    }

    // Update is called once per frame
    private void Update()
    {
        UpdateShieldFSM();
        UpdateAttack();
        UpdateSlowMo();
        UpdateDeath();
    }

    #region Attacking 

    [Header("Attack Attributes")]
    public float minTimeBetweenAttack = 0.05f;
    public float maxTimeBetweenAttack = 1.0f;
    private float _attackTimer = 0.0f;
    private bool _runAttackTimer = false;
    private bool _comboStart = true;

    private void UpdateAttack()
    {
        if (_comboStart)
        {
            if (_inputManager.GetAttackButtonPress() && shieldState != ShieldState.Shielding && _comboStart)
            {
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
        if(other.gameObject.CompareTag("EnemyWeapon"))
        {
            EnemyHandler enemy = other.gameObject.GetComponentInParent<EnemyHandler>();
            if (shieldState != ShieldState.Shielding || enemy.GetEnemyType() == EnemyHandler.EnemyType.ELITE)
            {
                _playerHandler.TakeDamage(enemy.GetDamage());
                enemy.weaponCollider.enabled = false;
            }
        }
        
        else if(other.gameObject.CompareTag("EnemyProjectile"))
        {
            EnemyHandler enemy = other.gameObject.GetComponent<EliteProjectile>().GetHandler();
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

        if (shieldCooldown <= 0)
        {
            shieldCooldown = _tempShieldCDTimer;
            shieldState = ShieldState.Default;
            _canShield = true;
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

    [Header("Slow Motion Attributes")]
    [Range(1, 100)]
    public int slowMotionPercentage = 50;
    public AnimationCurve tweenEase;
    private float tempSlowMoPercentage = 0.0f;
    private float currentTimeScale;
    private float defaultTimeScale = 1.0f;
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

    public bool Key_ActivateSlowMotion()
    {
        return activateSlowmo = true;
    }

    public bool Key_DeactivateSlowMotion()
    {
        return activateSlowmo = false;
    }

    #endregion

    #region Death
    [Header("Death Attributes")]
    public float timeTillRespawn = 3.0f;
    private bool _respawning = false;

    private void UpdateDeath()
    {
        // Make the player fade away as they take damage
        playerShader.material.SetFloat("_AlphaClip", _playerHandler.GetCurrentHealth());
        // Temp to force the player dead
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            _playerHandler.SetIsAlive(false);
            // Debug.Log("Player dead");
        }

        // Update if the player is alive or not
        if (_playerHandler.GetCurrentHealth() <= 0)
        {
            _playerHandler.SetIsAlive(false);
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
}
