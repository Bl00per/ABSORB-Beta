using UnityEngine;
using Cinemachine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraManager : MonoBehaviour
{
    // Player cameras
    public InputManager inputManager;
    public CinemachineFreeLook controllerCamera;
    public CinemachineFreeLook mouseCamera;
    public CinemachineVirtualCamera deathCamera;

    [HideInInspector]
    public Camera mainCamera;
    [HideInInspector]
    public Volume cameraVolume;
    [HideInInspector]
    public Vignette vignette;
    private bool _controllerUpdated = false;
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
        if (inputManager.GetIsUsingController())
            SetControllerCamera();
        // If no controller is connected or player has selected to use keyboard controls
        else if (!inputManager.GetIsUsingController())
            SetMouseCamera();

        mainCamera = Camera.main;
        cameraVolume = mainCamera.GetComponent<Volume>();
        cameraVolume.profile.TryGet(out vignette);
        _controllerUpdated = inputManager.GetIsUsingController();
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
        // (Basically if the user switches to controller from keyboard, the camera should change to accommodate for that)
        if (inputManager.GetIsUsingController() != _controllerUpdated)
        {
            // Check what input they are using now
            if (inputManager.GetIsUsingController())
            {
                controllerCamera.m_YAxis.Value = mouseCamera.m_YAxis.Value;
                controllerCamera.m_XAxis.Value = mouseCamera.m_XAxis.Value;
                SetControllerCamera();
            }
            else if (!inputManager.GetIsUsingController())
            {
                mouseCamera.m_YAxis.Value = controllerCamera.m_YAxis.Value;
                mouseCamera.m_XAxis.Value = controllerCamera.m_XAxis.Value;
                SetMouseCamera();
            }
            // Update this so this can be run once the input changes again
            _controllerUpdated = inputManager.GetIsUsingController();
        }   // If it detects that the controller was disconnected
        else if (inputManager.GetIsUsingController() && !inputManager.GetControllerConnected())
        {
            _pauseMenu.ShowControllerPopup();
            mouseCamera.m_YAxis.Value = controllerCamera.m_YAxis.Value;
            mouseCamera.m_XAxis.Value = controllerCamera.m_XAxis.Value;
            SetMouseCamera();
            inputManager.ManualUpdateController();
            _controllerUpdated = inputManager.GetIsUsingController();
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
        if (inputManager.GetIsUsingController())
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
        controllerCamera.m_YAxis.m_MaxSpeed = _tempControllerSpeedY;
        controllerCamera.m_XAxis.m_MaxSpeed = _tempControllerSpeedX;

        mouseCamera.m_YAxis.m_MaxSpeed = _tempMouseSpeedY;
        mouseCamera.m_XAxis.m_MaxSpeed = _tempMouseSpeedX;
    }

    public CinemachineFreeLook GetCurrentCamera()
    {
        if (mouseCamera.Priority > 0)
            return mouseCamera;
        else
            return controllerCamera;
    }

    public CinemachineFreeLook GetControllerCamera()
    {
        return controllerCamera;
    }

    public CinemachineFreeLook GetMouseCamera()
    {
        return mouseCamera;
    }

    public float GetVignetteIntensity()
    {
        return vignette.intensity.value;
    }

    public float SetVignetteIntensity(float intensity)
    {
        return vignette.intensity.value = intensity;
    }
}
