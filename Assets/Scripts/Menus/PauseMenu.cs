using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Audio;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject settingsMenu;
    public GameObject quitPopup;
    public GameObject controllerDisconnectedPopup;
    [Header("Settings Menu References")]
    public AudioMixer audioMixer; // Master, Music, SFX
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Toggle toggleButton;
    [Header("BUTTONS")]
    public GameObject pauseFirstSelectedButton;
    public GameObject settingFirstSelectedButton;
    public GameObject settingsClosedButton;
    public GameObject quitGameButton;

    [HideInInspector]
    public bool mouseControllerConfirmed;

    private ReadWriteText readWrite;
    private InputManager _inputManager;
    private CameraManager _cameraManager;
    private bool Paused;
    private MainMenu _mainMenu;
    private Image _masterSliderFill = null, _musicSliderFill = null, _sfxSliderFill = null;
    private GameObject quitPopupFirstSelectedButton;

    // Get file stuff on Awake
    void Awake()
    {
        readWrite = GetComponent<ReadWriteText>();
        SetSettingsOnStart();
    }

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
        quitPopupFirstSelectedButton = quitPopup.transform.GetChild(2).gameObject;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // This is so I don't have to assign every single fill
        // The fact you can't even change the highlight to be the fill instead of the handle is the bane of my existence
        _masterSliderFill = masterVolumeSlider.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>();
        _musicSliderFill = musicVolumeSlider.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>();
        _sfxSliderFill = sfxVolumeSlider.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>();
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
            if (settingsMenu.activeInHierarchy)
            {
                GameObject temp = EventSystem.current.currentSelectedGameObject;
                // I can't use a switch statement for this, I really tried. Forgive me
                if (temp == masterVolumeSlider.gameObject)
                    _masterSliderFill.color = new Vector4(213f, 0, 217f, 255f);
                else
                    _masterSliderFill.color = Color.white;

                if (temp == musicVolumeSlider.gameObject)
                    _musicSliderFill.color = new Vector4(213f, 0, 217f, 255f);
                else
                    _musicSliderFill.color = Color.white;

                if (temp == sfxVolumeSlider.gameObject)
                    _sfxSliderFill.color = new Vector4(213f, 0, 217f, 255f);
                else
                    _sfxSliderFill.color = Color.white;
            }
        }
        else if (controllerDisconnectedPopup.activeInHierarchy && _inputManager.GetControllerConnected())
        {
            ControllerReconnected();
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
            if (!_inputManager.GetControllerConnected() || _inputManager.GetOverrideController())
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
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
        EventSystem.current.SetSelectedGameObject(quitPopupFirstSelectedButton);
    }

    public void QuitConfirm()
    {
        Application.Quit();
        UnityEditor.EditorApplication.ExitPlaymode();
    }

    public void QuitDeny()
    {
        quitPopup.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(quitGameButton);
    }

    #endregion

    #region Settings Menu Functions

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
        SaveSettings();
        pauseMenu.SetActive(true);
        settingsMenu.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(settingsClosedButton);
    }

    public void ToggleOverrideControls()
    {
        _inputManager.SetOverrideController(!_inputManager.GetOverrideController());

        // If a controller isn't connected or override is toggled on
        if (!_inputManager.GetControllerConnected() || _inputManager.GetOverrideController())
        {
            _cameraManager.DisableCameraMovement();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            _cameraManager.DisableCameraMovement();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // Set all the settings for the game on start from the read data file (if it exists)
    private void SetSettingsOnStart()
    {
        /* Volume Sliders */
        masterVolumeSlider.value = readWrite.masterVolume;  // Master
        audioMixer.SetFloat("Master", masterVolumeSlider.value);

        musicVolumeSlider.value = readWrite.musicVolume;    // Music
        audioMixer.SetFloat("Music", musicVolumeSlider.value);

        sfxVolumeSlider.value = readWrite.sfxVolume;        // SFX
        audioMixer.SetFloat("SFX", sfxVolumeSlider.value);

        toggleButton.isOn = readWrite.overrideControls;     // Toggle override button
    }

    // Save all the settings values upon exiting the settings menu
    private void SaveSettings()
    {
        readWrite.masterVolume = masterVolumeSlider.value;
        readWrite.musicVolume = musicVolumeSlider.value;
        readWrite.sfxVolume = sfxVolumeSlider.value;
        readWrite.overrideControls = toggleButton.isOn;
        readWrite.OverwriteData();
    }

    // Set the volume of the master throught the slider
    public void SetMasterLvl(float level)
    {
        if (masterVolumeSlider.value == masterVolumeSlider.minValue)
            audioMixer.SetFloat("Master", -80f);    // Mute the volume 
        else
            audioMixer.SetFloat("Master", level);
    }

    // Set the volume of the Music throught the slider
    public void SetMusicLvl(float level)
    {
        // Mute the volume 
        if (musicVolumeSlider.value == musicVolumeSlider.minValue)
            audioMixer.SetFloat("Music", -80f);    // Mute the volume 
        else
            audioMixer.SetFloat("Music", level);
    }

    // Set the volume of the Effects throught the slider
    public void SetSFXLvl(float level)
    {
        // Mute the volume 
        if (sfxVolumeSlider.value == sfxVolumeSlider.minValue)
            audioMixer.SetFloat("SFX", -80f);    // Mute the volume 
        else
            audioMixer.SetFloat("SFX", level);
    }

    #endregion

    #region Controller Disconnected

    public void ControllerContinued()
    {
        _inputManager.EnableInput();
        _cameraManager.EnableCameraMovement();
        mouseControllerConfirmed = true;
        controllerDisconnectedPopup.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ShowControllerPopup()
    {
        _inputManager.DisableInput();
        _cameraManager.DisableCameraMovement();
        controllerDisconnectedPopup.SetActive(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ControllerReconnected()
    {
        _inputManager.EnableInput();
        _cameraManager.EnableCameraMovement();
        controllerDisconnectedPopup.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    #endregion
}