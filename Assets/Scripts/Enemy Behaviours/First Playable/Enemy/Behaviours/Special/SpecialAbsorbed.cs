using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialAbsorbed : AIBehaviour
{
    [Header("Graphic References")]
    public Material bodyMaterial;
    public Renderer bodyRenderer;
    public Material weaponMaterial;
    public Renderer weaponRenderer;

    [Header("VFX Refereneces")]
    public GameObject absorbGameObject;
    public ParticleSystem absorbParticleEffect;
    private SpecialParried _specialParried;
    private Animator _animator;

    [Header("Properties")]
    public float cutoffMax = 1.2f;
    public float cutOutSpeed = 1.0f;
    public float cutOffSpeed = 1.5f;
    public float destoryAbsorbEffectAfter = 4.0f;

    private bool _enabled = false;
    private float _cutOutTimer = 0.0f;
    private float _cutOffTimer = 0.0f;
    private float _defaultCutOut = 0.0f;
    private float _defaultCutOff = 0.0f;

    private void Awake()
    {
        // Getting the other behaviour
        _specialParried = this.GetComponent<SpecialParried>();

        // Getting the animator
        _animator = this.GetComponent<Animator>();
    }

    private void Start()
    {
        // Creating and assigning the new materials
        bodyRenderer.material = Instantiate(bodyMaterial);
        weaponRenderer.material = Instantiate(weaponMaterial);

        // // Storing the default values of the shader
        // _defaultCutOut = bodyRenderer.material.GetFloat("_Cutout");
        // _defaultCutOff = bodyRenderer.material.GetFloat("_Cutoff");
    }

    public override void OnStateEnter()
    {
        _animator.enabled = false;
        _cutOutTimer = cutoffMax;
        _enabled = true;
        absorbParticleEffect.Play();
        absorbGameObject.SetActive(true);
        absorbGameObject.transform.SetParent(null);
    }

    public override void OnStateExit() { }

    public override void OnStateFixedUpdate() { }

    public override void OnStateUpdate()
    {
        if (_enabled)
        {
            // if (_cutOutTimer > 0.7f)
            // {
            //     _cutOffTimer -= Time.deltaTime * cutOffSpeed;
            //     _cutOutTimer -= Time.deltaTime * cutOutSpeed;
            //     bodyRenderer.material.SetFloat("_Cutout", _cutOutTimer);
            //     weaponRenderer.material.SetFloat("_Cutoff", _cutOffTimer);
            // }
            // else
            // {
            //     absorbParticleEffect.Stop();
            //     absorbGameObject.SetActive(true);
            //     absorbGameObject.transform.SetParent(this.gameObject.transform);
            //     enemyHandler.Kill();
            //     _enabled = false;
            // }

            absorbParticleEffect.Stop();
            absorbGameObject.SetActive(true);
            absorbGameObject.transform.SetParent(this.gameObject.transform);
            enemyHandler.Kill();
            enemyHandler.Reset();
            _enabled = false;
        }
    }
}
