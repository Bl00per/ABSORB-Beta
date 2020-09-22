using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject settingsMenu;
    public Slider volumeSlider;
    public Toggle toggleButton;
    [Header("BUTTONS")]
    public GameObject pauseFirstSelectedButton;
    public GameObject settingFirstSelectedButton;
    public GameObject settingsClosedButton;

    private ReadWriteText readWrite;
    private InputManager _inputManager;
    private CameraManager _cameraManager;
    private bool Paused;
    private MainMenu _mainMenu;
    [SerializeField]
    private Image volumeFill;

    // Start is called before the first frame update
    void Start()
    {
        pauseMenu.SetActive(false);
        settingsMenu.SetActive(false);
        readWrite = GetComponent<ReadWriteText>();
        _inputManager = FindObjectOfType<InputManager>();
        _cameraManager = FindObjectOfType<CameraManager>();
        _mainMenu = GetComponent<MainMenu>();
        volumeSlider.value = readWrite.volume;
        toggleButton.isOn = readWrite.overrideControls;
    }

    // Update is called once per frame
    void Update()
    {
        // Make sure you can't pause when you die or the game is over
        if (_inputManager.GetPauseButtonPress() && !_mainMenu.inMainMenu && !settingsMenu.activeInHierarchy)
        {
            Pause();
        }
        // Check if you're in the settings menu
        else if (_inputManager.GetPauseButtonPress() && settingsMenu.activeInHierarchy)
        {
            CloseSettingsMenu();
        }

        if (Paused)
        {
            if (_inputManager.GetControllerConnected())
            {
                _inputManager._xciInputDirection.x = XboxCtrlrInput.XCI.GetAxisRaw(XboxCtrlrInput.XboxAxis.LeftStickX);
                _inputManager._xciInputDirection.y = XboxCtrlrInput.XCI.GetAxisRaw(XboxCtrlrInput.XboxAxis.LeftStickY);
            }
            
            if (EventSystem.current.currentSelectedGameObject == volumeSlider.gameObject)
            {
                volumeFill.color = new Vector4(213f, 0, 217f, 255f);
            }
            else
            {
                volumeFill.color = Color.white;
            }
        }

    }

    public void Pause()
    {
        if (Paused) // Game unpaused
        {
            Paused = false;
            _inputManager.EnableInput();
            pauseMenu.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            _cameraManager.EnableCameraMovement();
        }
        else // Game is paused
        {
            Paused = true;
            _inputManager.DisableInput();
            pauseMenu.SetActive(true);
            // Clear selected object
            EventSystem.current.SetSelectedGameObject(null);
            // Set the play button as the first selected object
            EventSystem.current.SetSelectedGameObject(pauseFirstSelectedButton);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            _cameraManager.DisableCameraMovement();
        }
    }

    public void ResumeGame()
    {
        Pause();
    }

    public void QuitGame()
    {
        // ADD A "ARE YOU SURE?" FIRST
        Application.Quit();
    }

    public void OpenSettingsMenu()
    {
        pauseMenu.SetActive(false);
        settingsMenu.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(settingFirstSelectedButton);
    }

    // Close the settings menu and write the changes to the file despite if a change was made or not
    public void CloseSettingsMenu()
    {
        readWrite.volume = volumeSlider.value;
        readWrite.overrideControls = toggleButton.isOn;
        readWrite.OverwriteData();
        pauseMenu.SetActive(true);
        settingsMenu.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(settingsClosedButton);
    }

    public void ToggleOverrideControls()
    {
        _inputManager.SetOverrideController(!_inputManager.GetOverrideController());
    }
}
