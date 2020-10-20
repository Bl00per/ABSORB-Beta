using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHandler : MonoBehaviour
{
    public enum PlayerAnimatorState
    {
        IDLE,
        ATTACK,
        SHIELD,
        DASH,
        ABSORB,
        DEATH,
    }
    private PlayerAnimatorState _currentState;

    // Attributes
    [Header("Attributes")]
    public float maxHealth = 100;
    public int primaryAttackDamage = 25;
    public float respawnFlyingTime = 5.0f;
    public float respawnFlyingOffset = 20.0f;
    [SerializeField]
    private float _currentHealth = 100;

    // References
    [Header("References")]
    public SkinnedMeshRenderer[] abidaroMesh;
    public GameObject respawnParticle;
    public ParticleSystem hitParticleSystem;
    public AudioSource hitSFX;
    private Animator _animator;
    private Rigidbody _rigidbody;
    private Transform _transform;
    private LocomotionHandler _locomotionHandler;
    private CombatHandler _combatHandler;
    private InputManager _inputManager;
    private SlowMotionManager _slowMotionManager;
    private CameraManager _cameraManager;
    private SimpleCameraShakeInCinemachine _simpleCameraShake;
    private CheckPoint _checkpoints;
    private Vector3 _respawnPosition;
    private CapsuleCollider _capsule;
    private float _checkPointYLevel;
    private float _playerYLevel;
    private int _defaultDamage;
    private AbilityHandler _abilityHandler;

    Vector3 point = Vector3.zero;
    Vector3 point2 = Vector3.zero;

    // Flags
    private bool isAlive = true;

    void Awake()
    {
        // Temporarily setting cursor locked state within this script.
        Cursor.lockState = CursorLockMode.Locked;

        // Getting the required components
        _animator = this.GetComponent<Animator>();
        _rigidbody = this.GetComponent<Rigidbody>();
        _transform = this.GetComponent<Transform>();
        _locomotionHandler = this.GetComponent<LocomotionHandler>();
        _combatHandler = this.GetComponent<CombatHandler>();
        _abilityHandler = this.GetComponent<AbilityHandler>();
        _inputManager = FindObjectOfType<InputManager>();
        _cameraManager = FindObjectOfType<CameraManager>();
        _slowMotionManager = FindObjectOfType<SlowMotionManager>();
        _simpleCameraShake = FindObjectOfType<SimpleCameraShakeInCinemachine>();
        _checkpoints = FindObjectOfType<CheckPoint>();
        _capsule = GetComponent<CapsuleCollider>();
        _currentHealth = maxHealth;
        _defaultDamage = primaryAttackDamage;
    }

    void Start()
    {
        // Set the player position as a respawn point
        SetRespawnPosition(this._transform.position);
        foreach (SkinnedMeshRenderer mesh in abidaroMesh)
        {
           // mesh.enabled = true;
        }
        respawnParticle.SetActive(false);
    }

    #region Player Respawn

    public void RespawnPlayer()
    {
        isAlive = true;
        _currentHealth = maxHealth;
        StartCoroutine(MoveOverSeconds(this.gameObject, GetRespawnPosition(), respawnFlyingOffset, respawnFlyingTime));
    }

    // Move the player back to checkpoint position over a certain number of seconds
    private IEnumerator MoveOverSeconds(GameObject objectToMove, Vector3 end, float height, float seconds)
    {
        float elapsedTime = 0;
        Vector3 startingPos = objectToMove.transform.position;

        float dynamicRespawnHeight = end.y;                                             // Get the Y level of the respawn point
        dynamicRespawnHeight = height + (_transform.position.y - dynamicRespawnHeight); // Compare the player & checkpoint height levels and add the offset
        if (dynamicRespawnHeight < 0)                                                   // If it turns out negative than multiply the offset x2 (Makes it look better, trust me)
        {
            dynamicRespawnHeight = (height * 2) + (_transform.position.y - dynamicRespawnHeight);
            Debug.Log("Dynamic height less than 0, increasing height");
        }

        // Stuff to disable while flying
        DisableReferences();

        while (elapsedTime < seconds)
        {
            objectToMove.transform.position = MathParabola.Parabola(startingPos, end,
            (dynamicRespawnHeight < 0) ? -dynamicRespawnHeight : dynamicRespawnHeight, (elapsedTime / seconds));    // This is just so we don't have an inverted parabola
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        // Turn that stuff back on after flight is complete
        EnableReferences();

        // Set to end position as a failsafe
        objectToMove.transform.position = end;
    }

    private void DisableReferences()
    {
        foreach (SkinnedMeshRenderer mesh in abidaroMesh)
        {
            mesh.enabled = false;
        }
        respawnParticle.SetActive(true);
        _capsule.enabled = false;
        _locomotionHandler.enabled = false;
        _cameraManager.deathCamera.Priority = 2;
        _rigidbody.useGravity = false;
    }

    private void EnableReferences()
    {
        foreach (SkinnedMeshRenderer mesh in abidaroMesh)
        {
            mesh.enabled = true;
        }
        _animator.SetBool("Death", false);
        respawnParticle.SetActive(false);
        _locomotionHandler.enabled = true;
        _cameraManager.deathCamera.Priority = 0;
        _capsule.enabled = true;
        _rigidbody.useGravity = true;
        _inputManager.EnableInput();
        _cameraManager.EnableCameraMovement(); // TEMPORARY FOR NOW
    }

    #endregion

    #region Getters and setters

    public bool GetIsAlive()
    {
        return isAlive;
    }

    public bool GetIsShielding()
    {
        return _combatHandler.shieldState == CombatHandler.ShieldState.Shielding;
    }

    public bool SetIsAlive(bool aliveState)
    {
        return isAlive = aliveState;
    }

    public float GetCurrentHealth()
    {
        return _currentHealth;
    }

    // Return the amount of damage the player should take
    public float TakeDamage(int damageAmount)
    {
        hitParticleSystem.Play();
        hitSFX.Play();
        _simpleCameraShake.Key_Shake_DAF("0.4|1.0|2.0");
        //collidedObject = null;
        // if (debug)
        //     Debug.Log("Player damage taken: " + damageAmount);

        //Debug.Log("Player damage taken: " + damageAmount);
        return _currentHealth -= damageAmount;
    }

    public void SetState(PlayerAnimatorState state)
    {
        _currentState = state;
    }

    public PlayerAnimatorState GetState()
    {
        return _currentState;
    }

    public Animator GetAnimator()
    {
        return _animator;
    }

    public InputManager GetInputManager()
    {
        return _inputManager;
    }

    public CameraManager GetCameraManager()
    {
        return _cameraManager;
    }

    public Rigidbody GetRigidbody()
    {
        return _rigidbody;
    }

    public Transform GetTransform()
    {
        return _transform;
    }

    public LocomotionHandler GetLocomotionHandler()
    {
        return _locomotionHandler;
    }

    public CombatHandler GetCombatHandler()
    {
        return _combatHandler;
    }

    public SlowMotionManager GetSlowMotionManager()
    {
        return _slowMotionManager;
    }

    public CheckPoint GetCheckPoint()
    {
        return _checkpoints;
    }

    public Vector3 GetRespawnPosition()
    {
        return _respawnPosition;
    }

    public Vector3 SetRespawnPosition(Vector3 checkpointPosition)
    {
        return _respawnPosition = checkpointPosition;
    }

    public int GetPrimaryAttackDamage()
    {
        return primaryAttackDamage;
    }

    public void SetPrimaryAttackDamage(int newDamage)
    {
        primaryAttackDamage = newDamage;
    }

    public void ResetPrimaryAttackDamage()
    {
        primaryAttackDamage = _defaultDamage;
    }

    public AbilityHandler GetAbilityHandler() => _abilityHandler;

    #endregion

    [HideInInspector]
    public bool shieldGrowing;
    public void Key_ShieldGrow()
    {
        shieldGrowing = true;
    }

    public void Key_ShieldShrink()
    {
        shieldGrowing = false;
    }
}