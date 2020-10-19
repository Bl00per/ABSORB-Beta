﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Audio;

public class PauseMenu : MonoBehaviour
{
    [Header("Menu References")]
    public GameObject pauseMenu;
    public GameObject settingsMenu;
    public GameObject quitPopup;
    public GameObject controllerDisconnectedPopup;
    [Header("Settings Menu References")]
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    [Header("Sensitivity Sliders")]
    public Slider sensitivityXSlider;
    public Slider sensitivityYSlider;
    public Text sensitivityMultiplierTextX;
    public Text sensitivityMultiplierTextY;
    [Header("BUTTONS")]
    public GameObject pauseFirstSelectedButton;
    public GameObject settingFirstSelectedButton;
    public GameObject settingsClosedButton;
    public GameObject quitGameButton;

    [HideInInspector]
    public bool mouseControllerConfirmed;

    private ReadWriteText _readWrite;
    private InputManager _inputManager;
    private CameraManager _cameraManager;
    private AudioMixer _audioMixer;
    private bool Paused;
    private MainMenu _mainMenu;
    private Image _masterSliderFill = null, _musicSliderFill = null, _sfxSliderFill = null, _sensXSliderFill = null, _sensYSliderFill = null;
    private GameObject _quitPopupFirstSelectedButton;
    private float cameraSensX = 0f;     // Keep track of current sensitivity
    private float cameraSensY = 0f;
    private float defaultSensX = 0f;    // For referencing to when setting sensitivity values
    private float defaultSensY = 0f;

    // Start is called before the first frame update
    void Start()
    {
        pauseMenu.SetActive(false);
        settingsMenu.SetActive(false);
        quitPopup.SetActive(false);
        controllerDisconnectedPopup.SetActive(false);
        _inputManager = FindObjectOfType<InputManager>();
        _cameraManager = FindObjectOfType<CameraManager>();
        _mainMenu = GetComponent<MainMenu>();
        _readWrite = GetComponent<ReadWriteText>();
        _audioMixer = _mainMenu.audioMixer;
        _quitPopupFirstSelectedButton = quitPopup.transform.GetChild(2).gameObject;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Update this if we have 2 different camera speed values
        defaultSensX = _cameraManager.mouseCamera.m_XAxis.m_MaxSpeed;
        defaultSensY = _cameraManager.mouseCamera.m_YAxis.m_MaxSpeed;
        // If you exit in and out of settings the camera speeds will be set to zero if this isnt here
        cameraSensX = defaultSensX;
        cameraSensY = defaultSensY;

        // This is so I don't have to assign every single fill
        // The fact you can't even change the highlight to be the fill instead of the handle is the bane of my existence
        _masterSliderFill = masterVolumeSlider.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>();
        _musicSliderFill = musicVolumeSlider.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>();
        _sfxSliderFill = sfxVolumeSlider.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>();
        _sensXSliderFill = sensitivityXSlider.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>();
        _sensYSliderFill = sensitivityYSlider.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>();
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
            ControllerRecognition();

            // Highlight the volume bar instead of the handle when it is highlighted
            if (settingsMenu.activeInHierarchy)
            {
                GameObject temp = EventSystem.current.currentSelectedGameObject;
                // I can't use a switch statement for this, I really tried. Forgive me
                if (temp == masterVolumeSlider.gameObject)  // Master Volume Slider
                    _masterSliderFill.color = new Vector4(213f, 0, 217f, 255f);
                else
                    _masterSliderFill.color = Color.white;

                if (temp == musicVolumeSlider.gameObject)   // Music Volume Slider
                    _musicSliderFill.color = new Vector4(213f, 0, 217f, 255f);
                else
                    _musicSliderFill.color = Color.white;

                if (temp == sfxVolumeSlider.gameObject)     // SFX Volume Slider
                    _sfxSliderFill.color = new Vector4(213f, 0, 217f, 255f);
                else
                    _sfxSliderFill.color = Color.white;

                if (temp == sensitivityXSlider.gameObject)  // Sensitivity X Slider
                    _sensXSliderFill.color = new Vector4(213f, 0, 217f, 255f);
                else
                    _sensXSliderFill.color = Color.white;

                if (temp == sensitivityYSlider.gameObject)  // Sensitivity Y Slider
                    _sensYSliderFill.color = new Vector4(213f, 0, 217f, 255f);
                else
                    _sensYSliderFill.color = Color.white;
            }
        }
    }

    #region Pause Menu Functions

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
            SetCameraSpeeds();
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
            // If the controller is connected dont display the cursor, it breaks things
            _cameraManager.DisableCameraMovement();
        }
    }

    public void ResumeGame()
    {
        Pause();
    }

    public void QuitGame()
    {
        // Enables "Are you sure?" popup
        quitPopup.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(_quitPopupFirstSelectedButton);
    }

    public void QuitConfirm()
    {
        Application.Quit();
        //UnityEditor.EditorApplication.ExitPlaymode();
    }

    public void QuitDeny()
    {
        quitPopup.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(quitGameButton);
    }

    private void ControllerRecognition()
    {
        // Show or hide the cursor during Pause depending on controller status and update in realtime
        if (_inputManager.GetIsUsingController())
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // If the selected button is ever set to null when using controller
            if (pauseMenu.activeInHierarchy && EventSystem.current.currentSelectedGameObject == null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(pauseFirstSelectedButton);
            }   // If you lose the selected button in the settings menu
            else if (settingsMenu.activeInHierarchy && EventSystem.current.currentSelectedGameObject == null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(settingFirstSelectedButton);
            }
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    #endregion

    #region Settings Menu Functions

    public void OpenSettingsMenu()
    {
        ReadSettings();
        pauseMenu.SetActive(false);
        settingsMenu.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(settingFirstSelectedButton);
    }

    // Close the settings menu and write the changes to the file despite if a change was made or not
    public void CloseSettingsMenu()
    {
        SaveSettings();
        pauseMenu.SetActive(true);
        settingsMenu.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(settingsClosedButton);
    }

    // Get the current volume settings (Useful for when we come from the main menu)
    private void ReadSettings()
    {
        masterVolumeSlider.value = _readWrite.masterVolume;
        musicVolumeSlider.value = _readWrite.musicVolume;
        sfxVolumeSlider.value = _readWrite.sfxVolume;
        sensitivityXSlider.value = _readWrite.sensitivityX;
        sensitivityYSlider.value = _readWrite.sensitivityY;
        sensitivityMultiplierTextX.text = (1 + (sensitivityXSlider.value / 10)).ToString("0.0");
        sensitivityMultiplierTextY.text = (1 + (sensitivityYSlider.value / 10)).ToString("0.0");
    }

    // Save all the settings values upon exiting the settings menu
    private void SaveSettings()
    {
        _readWrite.masterVolume = masterVolumeSlider.value;
        _readWrite.musicVolume = musicVolumeSlider.value;
        _readWrite.sfxVolume = sfxVolumeSlider.value;
        _readWrite.sensitivityX = sensitivityXSlider.value;
        _readWrite.sensitivityY = sensitivityYSlider.value;
        _readWrite.OverwriteData();
    }

    // Set the volume of the master throught the slider
    public void SetMasterLvl(float level)
    {
        if (masterVolumeSlider.value == masterVolumeSlider.minValue)
            _audioMixer.SetFloat("Master", -80f);    // Mute the volume 
        else
            _audioMixer.SetFloat("Master", level);
    }

    // Set the volume of the Music throught the slider
    public void SetMusicLvl(float level)
    {
        // Mute the volume 
        if (musicVolumeSlider.value == musicVolumeSlider.minValue)
            _audioMixer.SetFloat("Music", -80f);    // Mute the volume 
        else
            _audioMixer.SetFloat("Music", level);
    }

    // Set the volume of the Effects throught the slider
    public void SetSFXLvl(float level)
    {
        // Mute the volume 
        if (sfxVolumeSlider.value == sfxVolumeSlider.minValue)
            _audioMixer.SetFloat("SFX", -80f);    // Mute the volume 
        else
            _audioMixer.SetFloat("SFX", level);
    }

    // Sets the X Speed multiplier of the cameras
    public void SetCameraSensX(float multiplier)
    {
        cameraSensX = defaultSensX * (1 + (multiplier / 10));
        cameraSensX = defaultSensX * (1 + (multiplier / 10));
        sensitivityMultiplierTextX.text = (1 + (multiplier / 10)).ToString("0.0");
    }

    // Sets the Y Speed multiplier of the cameras
    public void SetCameraSensY(float multiplier)
    {
        cameraSensY = defaultSensY * (1 + (multiplier / 10));
        cameraSensY = defaultSensY * (1 + (multiplier / 10));
        sensitivityMultiplierTextY.text = (1 + (multiplier / 10)).ToString("0.0");
    }

    private void SetCameraSpeeds()
    {
        // Mouse Camera
        _cameraManager.mouseCamera.m_XAxis.m_MaxSpeed = cameraSensX;
        _cameraManager.mouseCamera.m_YAxis.m_MaxSpeed = cameraSensY;

        // Controller Camera
        _cameraManager.controllerCamera.m_XAxis.m_MaxSpeed = cameraSensX;
        _cameraManager.controllerCamera.m_YAxis.m_MaxSpeed = cameraSensY;
    }

    #endregion

    #region Controller Disconnected

    public void ShowControllerPopup()
    {
        controllerDisconnectedPopup.SetActive(true);
        StartCoroutine(ShowPopupForTime());
    }

    private IEnumerator ShowPopupForTime()
    {
        yield return new WaitForSeconds(3.0f);
        controllerDisconnectedPopup.SetActive(false);
    }

    #endregion
}