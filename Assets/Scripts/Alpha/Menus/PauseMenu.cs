using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject settingsMenu;
    public Slider volumeSlider;
    public Toggle toggleButton;
    private ReadWriteText readWrite;
    private InputManager _inputManager;
    private bool Paused;
    private MainMenu _mainMenu;

    #region Camera shizniz

    private CameraManager _cameraManager;

    #endregion

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
        if (Input.GetKeyDown(KeyCode.Escape) && !_mainMenu.inMainMenu && !settingsMenu.activeInHierarchy)
        {
            Pause();
        }
        // Check if you're in the settings menu
        else if (Input.GetKeyDown(KeyCode.Escape) && settingsMenu.activeInHierarchy)
        {
            CloseSettingsMenu();
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
    }

    // Close the settings menu and write the changes to the file despite if a change was made or not
    public void CloseSettingsMenu()
    {
        readWrite.volume = volumeSlider.value;
        readWrite.overrideControls = toggleButton.isOn;
        readWrite.OverwriteData();
        pauseMenu.SetActive(true);
        settingsMenu.SetActive(false);
    }

    public void ToggleOverrideControls()
    {
        _inputManager.SetOverrideController(!_inputManager.GetOverrideController());
    }
}
