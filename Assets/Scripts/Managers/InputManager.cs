﻿using UnityEngine;
using XboxCtrlrInput;
using Cinemachine;

public class InputManager : MonoBehaviour
{
    /* This is just here for copy and pasting
      private InputManager _inputManager;
      _inputManager = FindObjectOfType<InputManager>();
    */

    [Header("Properties")]
    public XboxController controller;
    public float inputTime = 0.1f;
    [Range(0, 1)]
    public float triggerDeadZone = 0.1F;
    [HideInInspector]
    public CinemachineFreeLook cinemachine;

    [Header("Attack Button")]
    public XboxButton attackXboxKey;
    public XboxButton altAttackXboxKey;
    public KeyCode attackKey;

    [Header("Special Attack Button")]
    public XboxButton splAttackXboxKey;
    public KeyCode splAttackKey;

    [Header("Shield Button")]
    public XboxButton shieldXboxKey;
    public XboxButton altShieldXboxKey;
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
    private bool _isUsingController;
    Timer timer;

    // Start is called before the first frame update
    void Awake()
    {
        controller = XboxController.First;
        _isUsingController = false; // Expecting player to use keyboard and mouse at start
        _cameraManager = FindObjectOfType<CameraManager>();
        timer = new Timer(inputTime);

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

        if (timer.ExpireReset())
            CheckForInputType();
    }

    void OnGUI()
    {
        Event e = Event.current;

        if (e.keyCode != 0 && _isUsingController)
        {
            _isUsingController = false;
            Debug.LogWarning("Input changed to keyboard");
        }
    }

    private void CheckForInputType()
    {
        // Check for input on controller and if the input isnt already on controller
        if ((PollControllerButtons() || PollControllerLJoystick() || PollControllerRJoystick()) && !_isUsingController)
        {
            _isUsingController = true;
            Debug.LogWarning("Input changed to controller");
        }   // Check for any keyboard key pressed and if the input isn't already on keyboard
        else if ((Input.GetAxis("Mouse X") > 0 || Input.GetAxis("Mouse X") < 0) && _isUsingController)
        {
            _isUsingController = false;
            Debug.LogWarning("Input changed to keyboard");
        }
    }

    private bool PollControllerButtons()
    {
        if (isControllerConnected)
        {
            for (int i = 0; i < (int)XboxButton.DPadRight + 1; ++i)
            {
                if (XCI.GetButtonDown((XboxButton)i, XboxController.First))
                    return true;
            }
        }

        return false;
    }

    private bool PollControllerLJoystick()
    {
        _xciInputDirection.x = XCI.GetAxisRaw(XboxAxis.LeftStickX);
        _xciInputDirection.y = XCI.GetAxisRaw(XboxAxis.LeftStickY);

        if (_xciInputDirection.magnitude > 0.1F)
            return true;
        else
            return false;
    }

    private bool PollControllerRJoystick()
    {
        _xciInputDirection.x = XCI.GetAxisRaw(XboxAxis.RightStickX);
        _xciInputDirection.y = XCI.GetAxisRaw(XboxAxis.RightStickY);

        if (_xciInputDirection.magnitude > 0.1F)
            return true;
        else
            return false;
    }

    public bool GetIsUsingController()
    {
        return _isUsingController;
    }

    // Toggle the isUsingContrller bool
    public void ManualUpdateController()
    {
        _isUsingController = !_isUsingController;
    }

    public Vector2 GetMovementDirectionFromInput()
    {
        if (!GetInputDisabled())
        {
            if (_isUsingController)
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
            return (_isUsingController) ? (XCI.GetButtonDown(attackXboxKey, XboxController.First) || XCI.GetButtonDown(altAttackXboxKey, XboxController.First)) : Input.GetKeyDown(attackKey);
        }
        else
            return false;
    }

    // Check for Special Attack button press
    public bool GetSpecialAttackButtonPress()
    {
        if (!GetInputDisabled())
        {
            return (_isUsingController) ? (XCI.GetButtonDown(splAttackXboxKey, XboxController.First) || GetRightTriggerDown()) : Input.GetKeyDown(splAttackKey);
        }
        else
            return false;
    }

    // Check for Shield button press
    public bool GetShieldButtonPress()
    {
        if (!GetInputDisabled())
        {
            return (_isUsingController) ? (XCI.GetButtonDown(shieldXboxKey, XboxController.First) || XCI.GetButtonDown(altShieldXboxKey, XboxController.First)) : Input.GetKeyDown(shieldKey);
        }
        else
            return false;
    }

    // Check for Dash button press
    public bool GetDashButtonPress()
    {
        if (!GetInputDisabled())
        {
            return (_isUsingController) ? (XCI.GetButtonDown(dashXboxKey, XboxController.First) || GetLeftTriggerDown()) : Input.GetKeyDown(dashKey);
        }
        else
            return false;
    }

    public bool GetLeftTriggerDown()
    {
        return XCI.GetAxis(XboxAxis.LeftTrigger, XboxController.First) > triggerDeadZone;
    }

    public bool GetRightTriggerDown()
    {
        return XCI.GetAxis(XboxAxis.RightTrigger, XboxController.First) > triggerDeadZone;
    }

    // Check for Pause button press
    public bool GetPauseButtonPress()
    {
        return (_isUsingController) ? XCI.GetButtonDown(pauseXboxKey, XboxController.First) : Input.GetKeyDown(pauseKey);
    }

    // Check for controller connected
    public bool GetControllerConnected()
    {
        return isControllerConnected;
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