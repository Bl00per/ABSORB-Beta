using UnityEngine;
using XboxCtrlrInput;
using Cinemachine;

public class InputManager : MonoBehaviour
{
    /* This is just here for copy and pasting
      private InputManager _inputManager;
      _inputManager = FindObjectOfType<InputManager>();
    */

    public XboxController controller;
    [HideInInspector]
    public CinemachineFreeLook cinemachine;

    [Header("Attack Button")]
    public XboxButton attackXboxKey;
    public KeyCode attackKey;

    [Header("Special Attack Button")]
    public XboxButton splAttackXboxKey;
    public KeyCode splAttackKey;

    [Header("Shield Button")]
    public XboxButton shieldXboxKey;
    public KeyCode shieldKey;

    [Header("Dash Button")]
    public XboxButton dashXboxKey;
    public KeyCode dashKey;

    [Header("Pause Button")]
    public XboxButton pauseXboxKey;
    public KeyCode pauseKey;

    private static bool _didQueryNumOfCtrlrs = false;
    private static bool isControllerConnected;

    private CameraManager _cameraManager;
    private Vector2 _unityInputDirection = Vector2.zero;
    private Vector2 _xciInputDirection = Vector2.zero;
    private int _queriedNumberOfCtrlrs;
    private bool _disableInput = false;  // Allow input as long as input isnt disabled


    // Start is called before the first frame update
    void Awake()
    {
        controller = XboxController.First;
        _cameraManager = FindObjectOfType<CameraManager>();

        // Check if there is a xbox controller connected on awake
        if (!_didQueryNumOfCtrlrs)
        {
            _didQueryNumOfCtrlrs = true;

            _queriedNumberOfCtrlrs = XCI.GetNumPluggedCtrlrs();
            if (_queriedNumberOfCtrlrs == 0)
            {
                //Debug.Log("No Xbox controllers plugged in!");
                isControllerConnected = false;
            }
            else
            {
                //Debug.Log(_queriedNumberOfCtrlrs + " Xbox controllers plugged in.");
                isControllerConnected = true;
                XCI.DEBUG_LogControllerNames();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Update isControllerConnected to allow the use of keyboard buttons 
        //if there is no controller connected or if a controller gets reconnected
        if (XCI.IsPluggedIn(1))
            isControllerConnected = true;
        else
            isControllerConnected = false;
    }

    public Vector2 GetMovementDirectionFromInput()
    {
        if (!GetInputDisabled())
        {
            if (isControllerConnected && !_cameraManager.overrideController)
                return UpdateXCIInputDirection();
            else
                return UpdateUnityInputDirection();
        }
        else
            return Vector2.zero;
    }

    public Vector2 UpdateUnityInputDirection()
    {
        _unityInputDirection.x = Input.GetAxisRaw("Horizontal");
        _unityInputDirection.y = Input.GetAxisRaw("Vertical");
        return _unityInputDirection;
    }

    public Vector2 UpdateXCIInputDirection()
    {
        _xciInputDirection.x = XCI.GetAxisRaw(XboxAxis.LeftStickX);
        _xciInputDirection.y = XCI.GetAxisRaw(XboxAxis.LeftStickY);
        return _xciInputDirection;
    }

    // Check for Attack button press
    public bool GetAttackButtonPress()
    {
        if (!GetInputDisabled())
        {
            if (_cameraManager.overrideController)
                return Input.GetKeyDown(attackKey);
            else
                return (isControllerConnected) ? XCI.GetButtonDown(attackXboxKey, XboxController.First) : Input.GetKeyDown(attackKey);
        }
        else
            return false;
    }

    // Check for Special Attack button press
    public bool GetSpecialAttackButtonPress()
    {
        if (!GetInputDisabled())
        {
            if (_cameraManager.overrideController)
                return Input.GetKeyDown(splAttackKey);
            else
                return (isControllerConnected) ? XCI.GetButtonDown(splAttackXboxKey, XboxController.First) : Input.GetKeyDown(splAttackKey);
        }
        else
            return false;
    }

    // Check for Shield button press
    public bool GetShieldButtonPress()
    {
        if (!GetInputDisabled())
        {
            if (_cameraManager.overrideController)
                return Input.GetKeyDown(shieldKey);
            else
                return (isControllerConnected) ? XCI.GetButtonDown(shieldXboxKey, XboxController.First) : Input.GetKeyDown(shieldKey);
        }
        else
            return false;
    }

    // Check for Dash button press
    public bool GetDashButtonPress()
    {
        if (!GetInputDisabled())
        {
            if (_cameraManager.overrideController)
                return Input.GetKeyDown(dashKey);
            else
                return (isControllerConnected) ? XCI.GetButtonDown(dashXboxKey, XboxController.First) : Input.GetKeyDown(dashKey);
        }
        else
            return false;
    }

    // Check for Pause button press
    public bool GetPauseButtonPress()
    {
        if (_cameraManager.overrideController)
            return Input.GetKeyDown(pauseKey);
        else
            return (isControllerConnected) ? XCI.GetButtonDown(pauseXboxKey, XboxController.First) : Input.GetKeyDown(pauseKey);
    }

    // Check for controller connected
    public bool GetControllerConnected()
    {
        return isControllerConnected;
    }

    public bool GetOverrideController()
    {
        return _cameraManager.overrideController;
    }

    public bool SetOverrideController(bool @override)
    {
        return _cameraManager.overrideController = @override;
    }

    // Gets the current state of disableInput boolean
    public bool GetInputDisabled()
    {
        return _disableInput;
    }

    // Changes the disableInput variable to allow key and controller input
    public bool EnableInput()
    {
        return _disableInput = false;
    }

    // Changes the disableInput variable to disable key and controller input
    public bool DisableInput()
    {
        return _disableInput = true;
    }
}