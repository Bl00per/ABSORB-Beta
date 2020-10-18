using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialParried : AIBehaviour
{
    [Header("References")]
    public Animator animator;
    public Renderer bodyRenderer;

    [Header("Properties")]
    public float exitTime = 3.0f;
    public bool lockState = false;
    public string stateReturnName = "Idle";

    [Header("Intensity Properties")]
    [Range(0f, 5f)]
    public float maxIntensity = 2f;
    [Range(0, 10)]
    public float flickerSpeed = 5f;

    // private AbilityManager playerAbililtyManager;

    private bool _isAbsorbable = false;
    private Material material;
    private Color specialColor;
    private float intensityCount = 0f;
    private float intensityTimer = 0f;

    private void Start()
    {
        material = bodyRenderer.material;
        specialColor = material.GetColor("_EmissionColor");
        // playerAbililtyManager = brain.PlayerTransform.GetComponent<AbilityManager>();

        //if (lockState)
        //playerAbililtyManager.SetAbsorbTarget(brain);
    }

    public override void OnStateEnter()
    {
        _isAbsorbable = true;
        animator?.SetBool("Parried", true);
        animator?.SetBool("Attacking", false);
        intensityCount = 5f;
        if (!lockState)
            StartCoroutine(ExitSequence());

        intensityTimer = 0f;
    }

    public override void OnStateExit()
    {
        // playerAbililtyManager.LastParriedEnemy = null;
        // playerAbililtyManager.SetAbsorbTarget(null);

        // Reset emission to default
        material.SetColor("_EmissionColor", specialColor);
        //material.SetColor("_EmissionColor", specialColor * 2f);

        animator?.SetBool("Parried", false);
        _isAbsorbable = false;
    }

    public override void OnStateFixedUpdate() { }

    public override void OnStateUpdate()
    {
        intensityTimer += (Time.timeScale / flickerSpeed);
        intensityCount = Mathf.PingPong(intensityTimer, maxIntensity);
        material.SetColor("_EmissionColor", specialColor * intensityCount);
        
        if (!_isAbsorbable)
        {
            brain.SetBehaviour(stateReturnName);
            // playerAbililtyManager.LastParriedEnemy = null;
            // playerAbililtyManager.SetAbsorbTarget(null);
        }
    }

    private IEnumerator ExitSequence()
    {
        yield return new WaitForSecondsRealtime(exitTime);
        _isAbsorbable = false;
    }

    public bool GetAbsorbable()
    {
        return _isAbsorbable;
    }
}
