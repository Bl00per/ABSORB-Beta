using System.Collections;
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
    public AudioSource buttonPressSound;
    [Header("Settings Menu References")]
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    [Header("Sensitivity Sliders")]
    public Slider sensitivityXSlider;
    public Slider sensitivityYSlider;
    public Text sensitivityMultiplierTextX;
    public Text sensitivityMultiplierTextY;
    public Color sliderColor;
    [Header("BUTTONS")]
    public GameObject pauseFirstSelectedButton;
    public GameObject settingFirstSelectedButton;
    public GameObject settingsClosedButton;
    public GameObject quitGameButton;

    [Header("ALL BUTTONS")]
    [SerializeField]
    private GameObject[] pauseMenuButtons;
    private Animator[] buttonAnimator;

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
    private RectTransform _quitButtonYes, _quitButtonNo;
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
        _quitButtonYes = quitPopup.transform.GetChild(1).GetComponent<RectTransform>();
        _quitButtonNo = _quitPopupFirstSelectedButton.GetComponent<RectTransform>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Update this if we have 2 different camera speed values
        defaultSensX = _cameraManager.mouseCamera.m_XAxis.m_MaxSpeed;
        defaultSensY = _cameraManager.mouseCamera.m_YAxis.m_MaxSpeed;
        // If you exit in and out of settings the camera speeds will be set to zero if this isnt here
        cameraSensX = defaultSensX;
        cameraSensY = defaultSensY;

        buttonAnimator = new Animator[pauseMenuButtons.Length];
        for (int i = 0; i < pauseMenuButtons.Length; i++)
        {
            buttonAnimator[i] = pauseMenuButtons[i].GetComponent<Animator>();
        }

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

    #region Pause Menu Functions

    public void Pause()
    {
        if (Paused) // Game unpaused
        {
            Paused = false;
            _inputManager.EnableInput();
            ResetButtonAnimator();
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
            ResetButtonAnimator();
            if (_inputManager.GetIsUsingController())
            {
                // Clear selected object
                EventSystem.current.SetSelectedGameObject(null);
                // Set the play button as the first selected object
                EventSystem.current.SetSelectedGameObject(pauseFirstSelectedButton);
            }
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
        if (_inputManager.GetIsUsingController())
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(_quitPopupFirstSelectedButton);
        }
    }

    public void QuitConfirm()
    {
        Application.Quit();
        //UnityEditor.EditorApplication.ExitPlaymode();
    }

    public void QuitDeny()
    {
        _quitButtonNo.localScale = new Vector3(1,1,1);
        _quitButtonYes.localScale = new Vector3(1,1,1);
        quitPopup.SetActive(false);
        if (_inputManager.GetIsUsingController())
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(quitGameButton);
        }
    }

    private void ResetButtonAnimator()
    {
        for (int i = 0; i < buttonAnimator.Length; i++)
        {
            buttonAnimator[i].SetTrigger("Normal");
            pauseMenuButtons[i].GetComponent<RectTransform>().localScale = new Vector3(1,1,1);
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

    public void PlayButtonSoundEffect()
    {
        buttonPressSound.Play();
    }

    #endregion

    #region Settings Menu Functions

    public void OpenSettingsMenu()
    {
        ReadSettings();
        ResetButtonAnimator();
        pauseMenu.SetActive(false);
        settingsMenu.SetActive(true);
        if (_inputManager.GetIsUsingController())
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(settingFirstSelectedButton);
        }
    }

    // Close the settings menu and write the changes to the file despite if a change was made or not
    public void CloseSettingsMenu()
    {
        SaveSettings();
        ResetButtonAnimator();
        pauseMenu.SetActive(true);
        settingsMenu.SetActive(false);
        if (_inputManager.GetIsUsingController())
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(settingsClosedButton);
        }
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