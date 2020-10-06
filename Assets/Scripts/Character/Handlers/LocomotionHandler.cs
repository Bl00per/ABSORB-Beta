using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocomotionHandler : MonoBehaviour
{
    /*
        This class will be used to control the "Locomotion" state machine within the 
        player controller.  
    */

    // References
    [Header("References")]
    public Transform mainPlayerCamera;
    private PlayerHandler _playerHandler;
    private InputManager _inputManager;
    private Rigidbody _rigidbody;
    private Transform _transform;
    private Animator _animator;
    private CapsuleCollider _collider;

    [Header("Movement Attributes")]
    public float acceleration = 100.0f;
    public float maxVelocity = 10.0f;
    [Range(0, 1)]
    public float turnSpeed = 0.20f;
    public float edgeOfWorldRayDistance = 5.0f;
    public float inAirGravity = 15.0f;
    public LayerMask edgeOfWorldLayerMask;
    private float _currentAcceleration = 0.0f;
    private bool _hasHitWall = false;
    private Vector3 _hasHitPosition = Vector3.zero;

    // Awake is called on initialise
    private void Awake()
    {
        // Setting the current acceleration
        _currentAcceleration = acceleration;
        _tempPlayerAcceleration = acceleration;
    }

    // Start is called before first frame
    private void Start()
    {
        // Getting the required references
        _playerHandler = this.GetComponent<PlayerHandler>();
        _rigidbody = _playerHandler.GetRigidbody();
        _transform = _playerHandler.GetTransform();
        _animator = _playerHandler.GetAnimator();
        _inputManager = _playerHandler.GetInputManager();
        _collider = this.GetComponent<CapsuleCollider>();
    }

    // Update is called once per frame
    private void Update()
    {
        // Updates the players slow down FSM
        UpdateSlowdownFSM();

        // Checks if the player is moving, and sets the animator accordingly
        // if (_rigidbody.velocity.magnitude > 3.5F)
        //     _animator.SetBool("Movement", true);
        // else if (_animator.GetBool("Movement"))
        //     _animator.SetBool("Movement", false);
    }

    // Fixed update is called every physics
    private void FixedUpdate()
    {
        // Updating the general movement
        FixedUpdateGeneralMovement();

        // Updating Dash
        UpdateDash();
    }

    #region General Movement

    private void FixedUpdateGeneralMovement()
    {
        // Updates the input direction from the scenes input manager
        Vector3 calculatedDirection = GetInputDirection().normalized;

        // Rotate player to face direction
        if (calculatedDirection.magnitude > 0.1F)
        {
            _transform.rotation = Quaternion.Slerp(_transform.rotation, Quaternion.LookRotation(calculatedDirection), turnSpeed);
            _animator.SetBool("Movement", true);
        }
        else
            _animator.SetBool("Movement", false);


        // Check if player is in the air, apply gravity if they are
        if (!CheckIfGrounded())
            _rigidbody.AddForce(Vector3.down * inAirGravity * Time.fixedDeltaTime, ForceMode.Impulse);

        // Check to see if the player is about to walk out of the map
        if (CheckForEdgeOfTerrain())
            return;

        PreventSlidingOnSlope();

        // Move player via forces
        if (_rigidbody.velocity.magnitude < maxVelocity)
            _rigidbody.AddForce(calculatedDirection * _currentAcceleration * Time.fixedDeltaTime, ForceMode.Impulse);
    }

    // Returns the forward direction of the camera.
    private Vector3 GetCalculatedForward()
    {
        // Get the camera forward
        Vector3 camPos = mainPlayerCamera.position;
        camPos.y = _transform.position.y;
        Vector3 cameraForward = (_transform.position - camPos).normalized;

        // Get the ground forward
        Vector3 groundForward = GetGroundForward(mainPlayerCamera.right);
        if (CheckIfGrounded() && groundForward.magnitude > 0)
            return cameraForward + groundForward;

        // Return cameras forward
        return cameraForward;
    }

    private Vector3 GetGroundForward(Vector3 rotAxis)
    {
        RaycastHit hit;
        if (Physics.SphereCast(_transform.position + (Vector3.up * 2), 0.2f, Vector3.down, out hit, 1.0f))
            return Quaternion.AngleAxis(90, rotAxis) * hit.normal;

        return Vector3.zero;
    }

    // Gets the ground under the player and checks for an incline, restircting any movement without input
    private void PreventSlidingOnSlope()
    {
        // If there is no input
        if (GetInputDirection().magnitude <= 0)
        {
            // Get ground below player
            RaycastHit hit = GetGroundBelowPlayer();

            // Setting the velocity of the rigidbody to 0 if we are on any incline
            if (hit.point.magnitude > 0 && hit.normal.y < 1)
                _rigidbody.velocity = Vector3.zero;
        }
    }

    private Vector3 GetInputDirection()
    {
        // Calculate the forward direction with a bunch of shitty vector math
        Vector3 inputDirection = _inputManager.GetMovementDirectionFromInput();
        Vector3 forward = GetCalculatedForward();
        Vector3 right = Quaternion.AngleAxis(90, Vector3.up) * forward;
        return (forward * inputDirection.y) + (right * inputDirection.x);
    }

    // Returns true if player collides with something at their feet
    private bool CheckIfGrounded()
    {
        RaycastHit hit;
        return Physics.SphereCast(_transform.position + (Vector3.up * 2), 0.2f, Vector3.down, out hit, 1.0f);
    }

    // Returns the collider below the players feet, if there is any
    private RaycastHit GetGroundBelowPlayer()
    {
        RaycastHit hit;
        if (Physics.SphereCast(_transform.position + (Vector3.up * 2), 0.2f, Vector3.down, out hit, 1.0f))
            return hit;

        return hit;
    }

    // Returns true if we are close enough to a wall, and have applied the slowdown
    // TODO: Maybe make this a sphere check around the player, and use the barriers forward to push the player away.
    private bool CheckForEdgeOfTerrain()
    {
        // Calculate the start point to the ray
        Vector3 start = _transform.position + (Vector3.up * _collider.height);

        // Check if we have hit an EOW barrier by casting a ray infront of the player
        RaycastHit hit;
        if (Physics.Raycast(start, _transform.TransformDirection(Vector3.forward), out hit, edgeOfWorldRayDistance, edgeOfWorldLayerMask))
        {
            // Get the inital position we hit the wall in
            if (!_hasHitWall)
            {
                _hasHitPosition = _transform.position;
                _hasHitWall = true;
            }

            // Get a value between 1 and 0, 1 being far away from the wall and 0 being close.
            float max = Vector3.Distance(_hasHitPosition, hit.point);
            float current = Vector3.Distance(_transform.position, hit.point);
            float value = max / current;

            // Negate the current velocity to slow down the player based on value
            _rigidbody.AddForce(-_rigidbody.velocity * value * Time.fixedDeltaTime, ForceMode.Impulse);

            // Returning true to declare we have hit the edge
            return true;
        }
        else
        {
            // Resetting the hit wall flag, and returning false when we don't connect
            _hasHitWall = false;
            return false;
        }
    }

    #endregion

    #region Slowdown

    public enum SlowState
    {
        Default,    // Default to chill on until another state is called
        Slowdown,   // Slow down the player
        SpeedUp     // Speed up the player
    }
    [HideInInspector]
    public SlowState slowState;

    [Header("Slowdown Properties")]
    public float maxSlowAcceleration = 10.0f;  // Lowest speed the player will be slowed down to
    public float timeToAcceleration = 0.2f;    // Approx time for the player acceleration to reach the slow/default amount

    private float _speedSmoothVelocity;
    private float _tempPlayerAcceleration;
    private bool _resetPlayerAcceleration = false;


    // Updates the slowdown
    public void UpdateSlowdownFSM()
    {
        switch (slowState)
        {
            case SlowState.Default:
                ResetPlayer();
                break;
            case SlowState.Slowdown:
                SlowdownPlayer();
                break;
            case SlowState.SpeedUp:
                SpeedUpPlayer();
                break;
        }
    }

    private void SlowdownPlayer()
    {
        _currentAcceleration = Mathf.SmoothDamp(_currentAcceleration, maxSlowAcceleration, ref _speedSmoothVelocity, timeToAcceleration);
    }

    private void SpeedUpPlayer()
    {
        _currentAcceleration = Mathf.SmoothDamp(_currentAcceleration, _tempPlayerAcceleration, ref _speedSmoothVelocity, timeToAcceleration);
        //if (!_resetPlayerAcceleration)

        // Failsafe to set back to default state so the slowdown state can be called again
        // It just makes sense that it goes back to normal once you reach normal speed again
        if (_currentAcceleration >= (_tempPlayerAcceleration * 0.95)) // When it's reached above 95% of the normal speed
        {
            _resetPlayerAcceleration = true;
            slowState = SlowState.Default;
        }
    }

    private void ResetPlayer()
    {
        if (_resetPlayerAcceleration)
        {
            _currentAcceleration = _tempPlayerAcceleration;
            _resetPlayerAcceleration = false;
        }
    }

    // Slows the player down to shield and absorb speed
    public void Key_ActivateSlowdown()
    {
        slowState = SlowState.Slowdown;
    }

    // Speeds the player back up to normal acceleration
    public void Key_DeactivateSlowdown()
    {
        slowState = SlowState.SpeedUp;
    }

    #endregion

    #region Dash

    public AudioSource dashSFX;
    public ParticleSystem dashPE;
    [Header("Dash Attributes")]
    public float force = 50.0f;
    public float cooldownTime = 5.0f;
    public float distance = 20.0f;
    public float smoothTime = 0.5f;
    public bool lerp = false;
    [Header("Debug Optionals")]
    public bool disableVelocityReset = false;
    public bool smoothDamp = false;
    private bool _canDash = true;
    private Vector3 _initialVelocity = Vector3.zero;
    private Vector3 _initialPosition = Vector3.zero;
    private bool _haveReset = false;

    private void UpdateDash()
    {
        if (_inputManager.GetDashButtonPress() && _canDash && _playerHandler.GetCombatHandler().shieldState != CombatHandler.ShieldState.Shielding)
        {
            _initialVelocity = _rigidbody.velocity;
            _initialPosition = transform.position;
            _rigidbody.AddForce(transform.forward * force, ForceMode.Impulse);
            _animator.SetBool("Dash", true);
            dashPE.Play();
            dashSFX.Play();
            _canDash = false;
            StartCoroutine(CoolDownSequence());
        }

        if (!_canDash)
        {
            if (Vector3.Distance(transform.position, _initialPosition) > distance && !_haveReset)
            {
                if (!disableVelocityReset)
                {
                    //_rigidbody.velocity = _initialVelocity;
                    if (smoothDamp)
                        _rigidbody.velocity = new Vector3(Mathf.SmoothDamp(_rigidbody.velocity.x, _initialVelocity.x, ref _speedSmoothVelocity, smoothTime), 0,
                        Mathf.SmoothDamp(_rigidbody.velocity.z, _initialVelocity.z, ref _speedSmoothVelocity, smoothTime));
                    else if (lerp)
                        _rigidbody.velocity = new Vector3(Mathf.LerpUnclamped(_rigidbody.velocity.x, _initialVelocity.x, smoothTime), 0,
                        Mathf.LerpUnclamped(_rigidbody.velocity.z, _initialVelocity.z, smoothTime));
                    else
                        _rigidbody.velocity = _initialVelocity;
                }
                _initialVelocity = Vector3.zero;
                _initialPosition = Vector3.zero;
                _animator.SetBool("Dash", false);
                _haveReset = true;
            }
        }
    }

    private IEnumerator CoolDownSequence()
    {
        yield return new WaitForSecondsRealtime(cooldownTime);
        _animator.SetBool("Dash", false);
        _canDash = true;
        _haveReset = false;
    }

    #endregion
}
