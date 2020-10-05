using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public Transform thirdPersonCamera;

    // The animator which controls the player
    private Animator _animator;

    // The rigidbody attached to the player
    private Rigidbody _rigidbody;

    // The input manager within the scene
    private InputManager _inputManager;

    // Caching the transform of the player
    private Transform _transform;

    // Awake is called on initialise
    void Awake()
    {
        // Temporarily setting cursor locked state within this script.
        Cursor.lockState = CursorLockMode.Locked;

        // Getting all required references
        _animator = this.GetComponent<Animator>();
        _rigidbody = this.GetComponent<Rigidbody>();
        _transform = this.GetComponent<Transform>();
        _inputManager = FindObjectOfType<InputManager>();
        _currentAcceleration = acceleration;
        _tempPlayerAcceleration = acceleration;
    }

    // Update is called once per frame
    void Update()
    {
        // Checks if the player is moving, and sets the animator accordingly
        if(_rigidbody.velocity.magnitude > 0.1F)
            _animator.SetBool("Movement", true);
        else
            _animator.SetBool("Movement", false);
    }

    // Fixed update called every physics tick
    void FixedUpdate()
    {
        UpdateSlowdownFSM();
        FixedUpdateGeneralMovement();
    }

    #region General Movement

    [Header("General Movement Properties")]
    public float acceleration = 100.0f;
    public float maxVelocity = 10.0f;
    public float turnSpeed = 0.20f;
    private float _currentAcceleration = 0.0f;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + Vector3.up, 0.2f);
    }

    // Returns the forward direction of the camera.
    Vector3 GetCalculatedForward()
    {
        // Get the camera forward
        Vector3 zeroCam = thirdPersonCamera.position;
        zeroCam.y = _transform.position.y;
        Vector3 cameraForward = (_transform.position - zeroCam).normalized;

        // Get the ground forward
        RaycastHit hit;
        if (Physics.SphereCast(_transform.position + Vector3.up, 0.2f, Vector3.down, out hit, 1.0f))
        {
            // Return calculated forward
            Vector3 groundForward = Quaternion.AngleAxis(90, thirdPersonCamera.right) * hit.normal;
            return cameraForward + groundForward;
        }

        // Return cameras forward
        return cameraForward;
    }

    public Vector3 GetInputDirection()
    {
        // Calculate the forward direction with a bunch of shitty vector math
        Vector3 inputDirection = _inputManager.GetMovementDirectionFromInput();
        Vector3 forward = GetCalculatedForward();
        Vector3 right = Quaternion.AngleAxis(90, Vector3.up) * forward;

        return (forward * inputDirection.y) + (right * inputDirection.x);
    }

    public void FixedUpdateGeneralMovement()
    {
        // Updates the input direction from the scenes input manager
        Vector3 calculatedDirection = GetInputDirection().normalized;

        // Move player via forces
        if (_rigidbody.velocity.magnitude < maxVelocity)
            _rigidbody.AddForce(calculatedDirection * _currentAcceleration * Time.fixedDeltaTime, ForceMode.Impulse);

        // Rotate player to face direction
        if (calculatedDirection.magnitude > 0.1F)
            _transform.rotation = Quaternion.Slerp(_transform.rotation, Quaternion.LookRotation(calculatedDirection), turnSpeed);
    }

    #endregion


    #region Slowdown   
    private enum SlowState
    {
        Default,    // Default to chill on until another state is called
        Slowdown,   // Slow down the player
        SpeedUp     // Speed up the player
    }
    private SlowState slowState;

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

    #endregion

    // Key Event function: Slows the player down to shield and absorb speed
    public void Key_ActivateSlowdown()
    {
        slowState = SlowState.Slowdown;
    }

    // Key Event function: Slows the player down to shield and absorb speed
    public void Key_DeactivateSlowdown()
    {
        slowState = SlowState.SpeedUp;
    }
}
