using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SlowMotionManager : MonoBehaviour
{
    [Header("Slow Motion Attributes")]
    [Range(1, 100)]
    public int slowMotionPercentage = 50;
    public AnimationCurve tweenEase;
    private float tempSlowMoPercentage = 0.0f;
    private float currentTimeScale;
    private float defaultTimeScale = 1.0f;
    private bool activateSlowmo = false;
    public AudioMixer _mixer;

    private enum SlowMotionState
    {
        None,
        Slowing,
        Speeding
    }
    private SlowMotionState slowState;

    private void Update()
    {
        tempSlowMoPercentage = slowMotionPercentage / 100.0f;

        if (slowState == SlowMotionState.Slowing)
        {
            _mixer.SetFloat("MasterPitch", 0.5f);
            currentTimeScale = Mathf.Lerp(currentTimeScale, tempSlowMoPercentage, tweenEase.Evaluate(Time.time));
            Time.timeScale = currentTimeScale;
        }
        else if (slowState == SlowMotionState.Speeding)
        {
            _mixer.SetFloat("MasterPitch", 1);
            //Debug.Log(_mixer.GetFloat("MasterPitch",))
            currentTimeScale = Mathf.Lerp(currentTimeScale, defaultTimeScale, 0.2f);
            if (currentTimeScale > 0.9f)
            {
                currentTimeScale = 1f;
                slowState = SlowMotionState.None;
            }
            Time.timeScale = currentTimeScale;
        }
    }

    public void ActivateSlowMotion()
    {
        slowState = SlowMotionState.Slowing;
    }

    public void DeactivateSlowMotion()
    {
        slowState = SlowMotionState.Speeding;
    }
}
