using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class EndCutscene : MonoBehaviour
{
    public GameObject player;
    public CinemachineVirtualCamera cutsceneCamera;
    public float timer;
    public float timer2;
    public GameObject logoImage;
    public GameObject[] objects;

    private BoxCollider _triggerBox;
    private PlayerHandler _playerHandler;
    private LocomotionHandler _locomotionHandler;
    private CombatHandler _combatHandler;
    private CinemachineFreeLook _mouseCamera;
    private CinemachineFreeLook _controllerCamera;

    // Start is called before the first frame update
    void Start()
    {
        _triggerBox = GetComponent<BoxCollider>();
        _mouseCamera = GameObject.Find("(Mouse)Third Person Camera").GetComponent<CinemachineFreeLook>();
        _controllerCamera = GameObject.Find("(Controller)Third Person Camera").GetComponent<CinemachineFreeLook>();
        _playerHandler = player.GetComponent<PlayerHandler>();
        _locomotionHandler = _playerHandler.GetLocomotionHandler();
        _combatHandler = _playerHandler.GetCombatHandler();

        foreach (GameObject gameObject in objects)
        {
            gameObject.SetActive(false);
        }
        logoImage.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            PlayEndCutscene();
    }

    // Remove all control and play cutscene
    private void PlayEndCutscene()
    {
        // Play animation?
        _mouseCamera.Priority = 0;
        cutsceneCamera.Priority = 1;
        _playerHandler.enabled = false;
        _combatHandler.enabled = false;
        _locomotionHandler.enabled = false;
        StartCoroutine(EnableObjects());
    }

    private IEnumerator EnableObjects()
    {
        yield return new WaitForSeconds(timer);
        foreach (GameObject gameObject in objects)
        {
            gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(timer2);
        logoImage.SetActive(true);
    }
}
