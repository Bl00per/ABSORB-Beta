using UnityEngine;
using Cinemachine;

public class Movement : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform;
    public CinemachineFreeLook freeLookCamera;
    private InputManager _inputManager;
    private Rigidbody _rigidbody;
    private Transform _transform;
    private Absorb _absorb;

    [Header("Attributes")]
    public float acceleration = 100.0f;
    public float maxVelocity = 10.0f;
    public float turnSpeed = 0.20f;

    // --
    private Vector2 _inputDirection = Vector2.zero;
    private Vector3 _direction = Vector3.zero;

    void Awake()
    {
        // Temporarily setting cursor locked state within this script.
        Cursor.lockState = CursorLockMode.Locked;

        // Getting components attached to gameobject.
        _rigidbody = this.GetComponent<Rigidbody>();
        _transform = this.GetComponent<Transform>();
        _absorb = this.GetComponent<Absorb>();
        _inputManager = FindObjectOfType<InputManager>();
    }

    // Returns the forward direction of the camera.
    Vector3 GetForwardViaCamera()
    {
        Vector3 flatCam = cameraTransform.position;
        flatCam.y = 0;
        return (_transform.position - flatCam).normalized;
    }

    void UpdateInputDirection()
    {

        //_inputDirection.x = Input.GetAxisRaw("Horizontal");
        //_inputDirection.y = Input.GetAxisRaw("Vertical");


        _inputDirection = _inputManager.GetMovementDirectionFromInput();



        // Get the forward direction
        Vector3 forward = GetForwardViaCamera();
        Vector3 right = Quaternion.AngleAxis(90, Vector3.up) * forward;
        _direction = (forward * _inputDirection.y) + (right * _inputDirection.x);
    }

    private void FixedUpdate()
    {
        // Updates the input direction from Unity's default input system.
        UpdateInputDirection();

        // Move player via forces
        if (_rigidbody.velocity.magnitude < maxVelocity)
            _rigidbody.AddForce(_direction.normalized * acceleration * Time.fixedDeltaTime, ForceMode.Impulse);

        // Rotate player to face direction
        if (_direction.magnitude > 0.1F)
            _transform.rotation = Quaternion.Slerp(_transform.rotation, Quaternion.LookRotation(_direction), turnSpeed);
    }

    private bool CheckForGroundViaRaycast()
    {
        return Physics.Raycast(_transform.position, -Vector3.up, 1.0f);
    }
}
