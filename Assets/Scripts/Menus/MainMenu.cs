using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Audio;
using Cinemachine;

public class MainMenu : MonoBehaviour
{
    public GameObject player;
    public GameObject mainMenu;
    public GameObject mainSettingsMenu;
    [SerializeField]
    public CinemachineVirtualCamera cutsceneCamera;
    [Header("Settings Menu References")]
    public AudioMixer audioMixer; // Master, Music, SFX
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Color sliderColor;
    [Header("Sensitivity Sliders")]
    public Slider sensitivityXSlider;
    public Slider sensitivityYSlider;
    public Text sensitivityMultiplierTextX;
    public Text sensitivityMultiplierTextY;
    [Header("BUTTONS")]
    public GameObject mainMenuFirstSelectedButton;
    public GameObject settingsFirstSelectedButton;
    public GameObject settingsClosedButton;
    [Header("For Debug Purposes")]
    public bool disableMenu = false;

    [Header("ALL BUTTONS")]
    [SerializeField]
    private GameObject[] menuButtons;
    private Animator[] buttonAnimator;

    private PlayerHandler _playerHandler;
    private LocomotionHandler _locomotionHandler;
    private CameraManager _cameraManager;
    private CombatHandler _combatHandler;
    private InputManager _inputManager;
    private ReadWriteText _readWrite;
    private Image _masterSliderFill = null, _musicSliderFill = null, _sfxSliderFill = null, _sensXSliderFill = null, _sensYSliderFill = null;
    [HideInInspector]
    public bool inMainMenu = true;
    private float cameraSensX = 0f;     // Keep track of current sensitivity
    private float cameraSensY = 0f;
    private float defaultSensX = 0f;    // For referencing to when setting sensitivity values
    private float defaultSensY = 0f;

    // Start is called before the first frame update
    void Start()
    {
        _readWrite = GetComponent<ReadWriteText>();
        SetSettingsOnStart();
        _playerHandler = player.GetComponent<PlayerHandler>();
        _locomotionHandler = _playerHandler.GetLocomotionHandler();
        _combatHandler = _playerHandler.GetCombatHandler();
        _cameraManager = _playerHandler.GetCameraManager();
        _readWrite = GetComponent<ReadWriteText>();
        _inputManager = FindObjectOfType<InputManager>();
        inMainMenu = true;
        mainSettingsMenu.SetActive(false);

        // // Clear selected object
        // EventSystem.current.SetSelectedGameObject(null);
        // // Set the play button as the first selected object
        // EventSystem.current.SetSelectedGameObject(mainMenuFirstSelectedButton);

        // Update this if we have 2 different camera speed values
        defaultSensX = _cameraManager.mouseCamera.m_XAxis.m_MaxSpeed;
        defaultSensY = _cameraManager.mouseCamera.m_YAxis.m_MaxSpeed;
        // If you exit in and out of settings the camera speeds will be set to zero if this isnt here
        cameraSensX = defaultSensX;
        cameraSensY = defaultSensY;

        buttonAnimator = new Animator[menuButtons.Length];
        for (int i = 0; i < menuButtons.Length; i++)
        {
            buttonAnimator[i] = menuButtons[i].GetComponent<Animator>();
        }

        _masterSliderFill = masterVolumeSlider.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>();
        _musicSliderFill = musicVolumeSlider.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>();
        _sfxSliderFill = sfxVolumeSlider.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>();
        _sensXSliderFill = sensitivityXSlider.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>();
        _sensYSliderFill = sensitivityYSlider.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>();

        // FOR DEBUG PURPOSES ONLY TO SKIP MENU
        if (!disableMenu)
        {
            cutsceneCamera.Priority = 2;
            mainMenu.SetActive(true);
            _playerHandler.enabled = false;
            _combatHandler.enabled = false;
            _locomotionHandler.enabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
            Initialise();
    }

    void Update()
    {
        if (inMainMenu)
        {
            ControllerRecognition();

            if (mainSettingsMenu.activeInHierarchy)
            {
                GameObject temp = EventSystem.current.currentSelectedGameObject;
                // I can't use a switch statement for this, I really tried. Forgive me
                if (temp == masterVolumeSlider.gameObject)  // Master Volume Slider
                    _masterSliderFill.color = sliderColor;
                else
                    _masterSliderFill.color = Color.white;

                if (temp == musicVolumeSlider.gameObject)   // Music Volume Slider
                    _musicSliderFill.color = sliderColor;
                else
                    _musicSliderFill.color = Color.white;

                if (temp == sfxVolumeSlider.gameObject)     // SFX Volume Slider
                    _sfxSliderFill.color = sliderColor;
                else
                    _sfxSliderFill.color = Color.white;

                if (temp == sensitivityXSlider.gameObject)  // Sensitivity X Slider
                    _sensXSliderFill.color = sliderColor;
                else
                    _sensXSliderFill.color = Color.white;

                if (temp == sensitivityYSlider.gameObject)  // Sensitivity Y Slider
                    _sensYSliderFill.color = sliderColor;
                else
                    _sensYSliderFill.color = Color.white;
            }
        }
    }

    // Plays the game
    public void Play()
    {
        Initialise();
    }

    // Exits the game
    public void Quit()
    {
        Application.Quit();
        //UnityEditor.EditorApplication.ExitPlaymode();
    }

    private void ResetButtonAnimator()
    {
        for (int i = 0; i < buttonAnimator.Length; i++)
        {
            buttonAnimator[i].SetTrigger("Normal");
            menuButtons[i].GetComponent<RectTransform>().localScale = new Vector3(1,1,1);
        }
    }

    // Displays the settings menu
    public void ShowSettings()
    {
        mainSettingsMenu.SetActive(true);
        ResetButtonAnimator();
        mainMenu.SetActive(false);
        if (_inputManager.GetIsUsingController())
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(settingsFirstSelectedButton);
        }
    }

    public void CloseSettings()
    {
        SaveSettings();
        mainMenu.SetActive(true);
        ResetButtonAnimator();
        mainSettingsMenu.SetActive(false);
        if (_inputManager.GetIsUsingController())
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(settingsClosedButton);
        }
    }

    private void ControllerRecognition()
    {
        // Show or hide the cursor during Pause depending on controller status and update in realtime
        if (_inputManager.GetIsUsingController())
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            // If the selected button is ever set to null when using controller
            if (mainMenu.activeInHierarchy && EventSystem.current.currentSelectedGameObject == null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(mainMenuFirstSelectedButton);
            }   // If you lose the selected button in the settings menu
            else if (mainSettingsMenu.activeInHierarchy && EventSystem.current.currentSelectedGameObject == null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(settingsFirstSelectedButton);
            }
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // Set all the settings for the game on start from the read data file (if it exists)
    private void SetSettingsOnStart()
    {
        /* Volume Sliders */
        masterVolumeSlider.value = _readWrite.masterVolume;     // Master
        audioMixer.SetFloat("Master", masterVolumeSlider.value);

        musicVolumeSlider.value = _readWrite.musicVolume;       // Music
        audioMixer.SetFloat("Music", musicVolumeSlider.value);

        sfxVolumeSlider.value = _readWrite.sfxVolume;           // SFX
        audioMixer.SetFloat("SFX", sfxVolumeSlider.value);

        sensitivityXSlider.value = _readWrite.sensitivityX;     // Sensitivity X
        sensitivityMultiplierTextX.text = (1 + (sensitivityXSlider.value / 10)).ToString("0.0");

        sensitivityYSlider.value = _readWrite.sensitivityY;     // Sensitivity Y
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

    // Initialise all the variables that have to be set for the mouse controls
    private void Initialise()
    {
        cutsceneCamera.Priority = 0;
        _playerHandler.enabled = true;
        _combatHandler.enabled = true;
        _locomotionHandler.enabled = true;
        mainMenu.SetActive(false);
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
        inMainMenu = false;
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
}