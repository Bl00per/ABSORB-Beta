using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArenaBarrier : MonoBehaviour
{

    [Header("Barrier References")]
    public ObjectPooler objectPooler;
    private PlayerHandler playerHandler;
    [SerializeField]
    private GameObject barrier;
    [SerializeField]
    private AudioSource barrierSoundEffect;
    public Trigger triggerBox;
    public AudioSource battleMusic;
    public float musicFadeOutTime;

    private MeshRenderer _barrierMesh;
    private Collider _barrierCollider;
    private Collider _hiddenBarrierCollider;
    private bool _barrierDisabled = false;
    private bool _barrierTriggered = false;
    private bool _startedBarrierDeactivate = false;
    private float _volumeHolder;

    // Start is called before the first frame update
    void Start()
    {
        playerHandler = FindObjectOfType<PlayerHandler>();

        _barrierMesh = barrier?.GetComponent<MeshRenderer>();
        _barrierCollider = barrier?.GetComponent<Collider>();
        _hiddenBarrierCollider = barrier?.transform.GetChild(0).GetComponent<Collider>();
        BarrierInactive();

        _volumeHolder = battleMusic.volume;
    }

    // Update is called once per frame
    void Update()
    {
        // Checking for when player touches the barrier trigger
        PlayerActivateBarrier();

        if (!playerHandler.GetIsAlive())
        {
            StartCoroutine(ResetBarrier());
        }
        // If final enemy is dead or doesn't exist, disable the barrier
        else if ((!CheckForFinalEnemy() && !_barrierDisabled))
        {
            StartCoroutine(DeactivateBarrier());
        }
    }

    #region Barrier Functions

    private void PlayerActivateBarrier()
    {
        if (triggerBox.Collider != null)
        {
            if (triggerBox.Collider.CompareTag("Player") && playerHandler.GetIsAlive() && triggerBox.Enabled && !_barrierTriggered)
            {
                // triggerBox.GetComponent<Collider>().isTrigger = false;
                BarrierActive();
                barrierSoundEffect.Play();
                battleMusic.Play();
                battleMusic.volume = _volumeHolder;
                _barrierTriggered = true;
                triggerBox.Collider = null;
            }
        }
    }

    private bool CheckForFinalEnemy()
    {
        if (!objectPooler.finalEnemy.IsAlive() || objectPooler.finalEnemy == null)
            return false;
        else
            return true;
    }

    private IEnumerator DeactivateBarrier()
    {
        //triggerBox.GetComponent<Collider>().isTrigger = true;
        barrierSoundEffect.Play();
        _barrierDisabled = true;
        BarrierInactive();

        float elapsedTime = 0;
        while (elapsedTime < musicFadeOutTime)
        {
            float volume = Mathf.Lerp(battleMusic.volume, 0, (elapsedTime / musicFadeOutTime));
            battleMusic.volume = volume;
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        battleMusic.Stop();
    }

    private IEnumerator ResetBarrier()
    {
        _barrierTriggered = false;
        barrierSoundEffect.Play();
        BarrierInactive();

        float elapsedTime = 0;
        while (elapsedTime < musicFadeOutTime)
        {
            float volume = Mathf.Lerp(battleMusic.volume, 0, (elapsedTime / musicFadeOutTime));
            battleMusic.volume = volume;
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        battleMusic.Stop();
        battleMusic.volume = _volumeHolder;
    }

    private void BarrierActive()
    {
        _barrierCollider.enabled = true;
        _hiddenBarrierCollider.enabled = true;
        _barrierMesh.enabled = true;
    }

    private void BarrierInactive()
    {
        _barrierCollider.enabled = false;
        _hiddenBarrierCollider.enabled = false;
        _barrierMesh.enabled = false;
    }

    #endregion
}
