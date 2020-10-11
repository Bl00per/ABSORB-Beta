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
    public int maxHealth = 100;
    public int primaryAttackDamage = 25;
    public float respawnFlyingTime = 5.0f;
    public float respawnFlyingOffset = 20.0f;
    private int _currentHealth = 100;

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
    private CameraManager _cameraManager;
    private CheckPoint _checkpoints;
    private Vector3 _respawnPosition;
    private CapsuleCollider _capsule;
    private float offset = 10.0f;
    private float _checkPointYLevel;
    private float _playerYLevel;
    private int _defaultDamage;

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
        _inputManager = FindObjectOfType<InputManager>();
        _cameraManager = FindObjectOfType<CameraManager>();
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
            mesh.enabled = true;
        }
        respawnParticle.SetActive(false);
    }

    #region Player Respawn

    public void RespawnPlayer()
    {
        isAlive = true;
        _currentHealth = maxHealth;
        StartCoroutine(MoveOverSeconds(this.gameObject, GetRespawnPosition(), respawnFlyingOffset, respawnFlyingTime));
        //_transform.rotation = GetRespawnPosition().rotation;
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

    // Get a mid point between two positions
    private Vector3 GetPoint(Vector3 object1, Vector3 object2)
    {
        //get the positions of our transforms
        Vector3 pos1 = object1;
        Vector3 pos2 = object2;

        //get the direction between the two transforms -->
        Vector3 dir = (pos2 - pos1).normalized;

        //get a direction that crosses our [dir] direction
        //NOTE! : this can be any of a buhgillion directions that cross our [dir] in 3D space
        //To alter which direction we're crossing in, assign another directional value to the 2nd parameter
        Vector3 perpDir = Vector3.Cross(dir, Vector3.right);

        //get our midway point
        Vector3 midPoint = (pos1 + pos2) / 2f;

        //get the offset point
        //This is the point you're looking for.
        Vector3 offsetPoint = midPoint + (perpDir * offset);

        return offsetPoint;
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

    public int GetCurrentHealth()
    {
        return _currentHealth;
    }

    // Return the amount of damage the player should take
    public float TakeDamage(int damageAmount)
    {
        hitParticleSystem.Play();
        hitSFX.Play();
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
        // Debug.Log("Set new respawn position at: (" + checkpointPosition.x + ", " +
        // checkpointPosition.y + ", " + checkpointPosition.z + ")");
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

    #endregion

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