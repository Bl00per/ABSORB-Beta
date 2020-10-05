using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Cinemachine;

public class MainMenu : MonoBehaviour
{
    public GameObject player;
    public GameObject mainMenu;
    [SerializeField]
    public CinemachineVirtualCamera cutsceneCamera;
    [Header("For Debug Purposes")]
    public bool disableMenu = false;

    private PlayerHandler _playerHandler;
    private LocomotionHandler _locomotionHandler;
    private CombatHandler _combatHandler;
    private CinemachineFreeLook _mouseCamera;
    private CinemachineFreeLook _controllerCamera;
    private InputManager _inputManager;
    private CameraManager _cameraManager;
    private GameObject _firstSelectedButton;
    [HideInInspector]
    public bool inMainMenu = true;

    // Start is called before the first frame update
    void Start()
    {
        _playerHandler = player.GetComponent<PlayerHandler>();
        _inputManager = _playerHandler.GetInputManager();
        _locomotionHandler = _playerHandler.GetLocomotionHandler();
        _combatHandler = _playerHandler.GetCombatHandler();
        _cameraManager = FindObjectOfType<CameraManager>();
        inMainMenu = true;

        // Dont think too much about it
        _firstSelectedButton = this.transform.GetChild(0).GetChild(0).GetChild(0).gameObject;
        // Clear selected object
        EventSystem.current.SetSelectedGameObject(null);
        // Set the play button as the first selected object
        EventSystem.current.SetSelectedGameObject(_firstSelectedButton);

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

    // Plays the game
    public void Play()
    {
        Initialise();
        // // If the controller isnt connected use the mouse camera
        // if (!_inputManager.GetControllerConnected())
        //     InitialiseMouse();
        // else
        //     InitialiseController();
    }

    // Exits the game
    public void Quit()
    {
        Application.Quit();
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

    // // Initialise all the variables that have to be set for the mouse controls
    // private void InitialiseMouse()
    // {
    //     _cameraManager.SetMouseCamera();
    //     cutsceneCamera.Priority = 0;
    //     _playerHandler.enabled = true;
    //     _combatHandler.enabled = true;
    //     _locomotionHandler.enabled = true;
    //     mainMenu.SetActive(false);
    //     Cursor.lockState = CursorLockMode.Confined;
    //     Cursor.visible = false;
    //     inMainMenu = false;
    // }

    // // Initialise all the variables that have to be set for the controller controls
    // private void InitialiseController()
    // {
    //     _cameraManager.SetControllerCamera();
    //     cutsceneCamera.Priority = 0;
    //     _playerHandler.enabled = true;
    //     _combatHandler.enabled = true;
    //     _locomotionHandler.enabled = true;
    //     mainMenu.SetActive(false);
    //     Cursor.lockState = CursorLockMode.Confined;
    //     Cursor.visible = false;
    //     inMainMenu = false;
    // }
}