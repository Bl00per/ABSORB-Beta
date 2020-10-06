using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    // Player cameras
    public InputManager inputManager;
    public CinemachineFreeLook controllerCamera;
    public CinemachineFreeLook mouseCamera;
    public CinemachineVirtualCamera deathCamera;
    [Header("Only allow Keyboard input")]
    [HideInInspector]
    public bool overrideController = false;

    private bool _controllerUpdated = false;
    private bool _overrideUpdated = false;
    private PauseMenu _pauseMenu;

    // Speed shit, please ignore
    private float _tempMouseSpeedY;
    private float _tempMouseSpeedX;
    private float _tempControllerSpeedY;
    private float _tempControllerSpeedX;

    // Start is called before the first frame update
    void Start()
    {
        //inputManager = FindObjectOfType<InputManager>();
        _pauseMenu = FindObjectOfType<PauseMenu>();

        // If controller is connected and not overridden
        if (inputManager.GetControllerConnected() && !overrideController)
            SetControllerCamera();
        // If no controller is connected or player has selected to use keyboard controls
        else if (!inputManager.GetControllerConnected() || overrideController)
            SetMouseCamera();

        _controllerUpdated = inputManager.GetControllerConnected();
        _overrideUpdated = overrideController;
        deathCamera.Priority = 0;

        _tempMouseSpeedY = mouseCamera.m_YAxis.m_MaxSpeed;
        _tempMouseSpeedX = mouseCamera.m_XAxis.m_MaxSpeed;
        _tempControllerSpeedY = controllerCamera.m_YAxis.m_MaxSpeed;
        _tempControllerSpeedX = controllerCamera.m_XAxis.m_MaxSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        // Run when the inputManager and controllerUpdated dont match
        // (Basically this should on run once when a controller is connected/disconnected)
        // Also run if overrideController has been turned on/off
        if (inputManager.GetControllerConnected() != _controllerUpdated || overrideController != _overrideUpdated)
        {
            if (inputManager.GetControllerConnected() && !overrideController)
            {
                controllerCamera.m_YAxis.Value = mouseCamera.m_YAxis.Value;
                controllerCamera.m_XAxis.Value = mouseCamera.m_XAxis.Value;
                _controllerUpdated = inputManager.GetControllerConnected();
                SetControllerCamera();
            }
            else if (overrideController)
            {
                // Only set the position if the controller is connected
                if (inputManager.GetControllerConnected())
                {
                    mouseCamera.m_YAxis.Value = controllerCamera.m_YAxis.Value;
                    mouseCamera.m_XAxis.Value = controllerCamera.m_XAxis.Value;
                }
                _controllerUpdated = inputManager.GetControllerConnected();
                SetMouseCamera();
            }
            else if (!inputManager.GetControllerConnected() && overrideController == _overrideUpdated)
            {
                _pauseMenu.ShowControllerPopup();
                if (_pauseMenu.mouseControllerConfirmed)
                {
                    mouseCamera.m_YAxis.Value = controllerCamera.m_YAxis.Value;
                    mouseCamera.m_XAxis.Value = controllerCamera.m_XAxis.Value;
                    _pauseMenu.mouseControllerConfirmed = false;
                    _controllerUpdated = inputManager.GetControllerConnected();
                    SetMouseCamera();
                }
            }

            _overrideUpdated = overrideController;
            Debug.LogWarning("Controller changed");
        }
    }

    public void SetControllerCamera()
    {
        // Change Camera Priority
        controllerCamera.Priority = 1;
        mouseCamera.Priority = 0;

        // Set InputManager camera
        inputManager.cinemachine = controllerCamera;
    }

    public void SetMouseCamera()
    {
        // Change Camera Priority
        mouseCamera.Priority = 1;
        controllerCamera.Priority = 0;

        // Set InputManager camera
        inputManager.cinemachine = mouseCamera;
    }

    public void DisableCameraMovement()
    {
        // Set the speed of the cameras to 0 so you can't move them
        if (inputManager.GetControllerConnected())
        {
            controllerCamera.m_YAxis.m_MaxSpeed = 0f;
            controllerCamera.m_XAxis.m_MaxSpeed = 0f;
        }
        else
        {
            mouseCamera.m_YAxis.m_MaxSpeed = 0f;
            mouseCamera.m_XAxis.m_MaxSpeed = 0f;
        }
    }

    public void EnableCameraMovement()
    {
        // Set the speed of the cameras back to default
        if (inputManager.GetControllerConnected())
        {
            controllerCamera.m_YAxis.m_MaxSpeed = _tempControllerSpeedY;
            controllerCamera.m_XAxis.m_MaxSpeed = _tempControllerSpeedX;
        }
        else
        {
            mouseCamera.m_YAxis.m_MaxSpeed = _tempMouseSpeedY;
            mouseCamera.m_XAxis.m_MaxSpeed = _tempMouseSpeedX;
        }
    }
}
