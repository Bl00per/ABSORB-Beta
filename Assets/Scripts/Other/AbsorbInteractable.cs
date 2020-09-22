using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbsorbInteractable : MonoBehaviour
{
    [Header("Refereneces")]
    public BoxCollider boxCollider;
    public Animator animator;
    public Light lightComponent;
    public LightFlicker flicker;
    public GameObject particleEffect;
    private bool _isAbsorbable = true;

    [Header("Properties")]
    public float particleEffectTime = 3.0f;

    public void Activate()
    {
        flicker.Disable();
        particleEffect.SetActive(true);
        lightComponent.intensity = 0;
        animator.SetBool("DoorTriggered", true);
        boxCollider.enabled = false;
        _isAbsorbable = false;
        StartCoroutine(ParticleEffectTime());
    }

    private IEnumerator ParticleEffectTime()
    {
        yield return new WaitForSeconds(particleEffectTime);
        particleEffect.SetActive(false);
    }

    public bool IsAbsorbable()
    {
        return _isAbsorbable;
    }
}