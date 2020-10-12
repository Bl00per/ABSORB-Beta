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
    [Header("BUTTONS")]
    public GameObject mainMenuFirstSelectedButton;
    public GameObject settingsFirstSelectedButton;
    public GameObject settingsClosedButton;
    [Header("For Debug Purposes")]
    public bool disableMenu = false;

    private PlayerHandler _playerHandler;
    private LocomotionHandler _locomotionHandler;
    private CombatHandler _combatHandler;
    private InputManager _inputManager;
    private ReadWriteText _readWrite;
    private Image _masterSliderFill = null, _musicSliderFill = null, _sfxSliderFill = null;
    [HideInInspector]
    public bool inMainMenu = true;

    // Start is called before the first frame update
    void Start()
    {
        _readWrite = GetComponent<ReadWriteText>();
        SetSettingsOnStart();
        _playerHandler = player.GetComponent<PlayerHandler>();
        _locomotionHandler = _playerHandler.GetLocomotionHandler();
        _combatHandler = _playerHandler.GetCombatHandler();
        _readWrite = GetComponent<ReadWriteText>();
        _inputManager = FindObjectOfType<InputManager>();
        inMainMenu = true;
        mainSettingsMenu.SetActive(false);

        // Clear selected object
        EventSystem.current.SetSelectedGameObject(null);
        // Set the play button as the first selected object
        EventSystem.current.SetSelectedGameObject(mainMenuFirstSelectedButton);

        _masterSliderFill = masterVolumeSlider.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>();
        _musicSliderFill = musicVolumeSlider.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>();
        _sfxSliderFill = sfxVolumeSlider.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>();

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

    // Displays the settings menu
    public void ShowSettings()
    {
        mainSettingsMenu.SetActive(true);
        mainMenu.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(settingsFirstSelectedButton);
    }

    public void CloseSettings()
    {
        SaveSettings();
        mainSettingsMenu.SetActive(false);
        mainMenu.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(settingsClosedButton);
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
        masterVolumeSlider.value = _readWrite.masterVolume;  // Master
        audioMixer.SetFloat("Master", masterVolumeSlider.value);

        musicVolumeSlider.value = _readWrite.musicVolume;    // Music
        audioMixer.SetFloat("Music", musicVolumeSlider.value);

        sfxVolumeSlider.value = _readWrite.sfxVolume;        // SFX
        audioMixer.SetFloat("SFX", sfxVolumeSlider.value);
    }

    // Save all the settings values upon exiting the settings menu
    private void SaveSettings()
    {
        _readWrite.masterVolume = masterVolumeSlider.value;
        _readWrite.musicVolume = musicVolumeSlider.value;
        _readWrite.sfxVolume = sfxVolumeSlider.value;
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
}