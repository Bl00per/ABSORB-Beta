using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Events;

public class SimpleCameraShakeInCinemachine : MonoBehaviour
{

    public float shakeDuration = 0.3f;          // Time the Camera Shake effect will last
    public float shakeAmplitude = 1.2f;         // Cinemachine Noise Profile Parameter
    public float shakeFrequency = 2.0f;         // Cinemachine Noise Profile Parameter

    // Cinemachine Shake
    public CinemachineFreeLook cmFreeCam;

    private CinemachineBasicMultiChannelPerlin[] _cinemachineBasicMultiChannelPerlin;

    private void Awake()
    {
        // Creating an array to store the rigs in, to prevent using get component every Shake()
        _cinemachineBasicMultiChannelPerlin = new CinemachineBasicMultiChannelPerlin[3];
        for (int i = 0; i < 3; ++i)
            _cinemachineBasicMultiChannelPerlin[i] = cmFreeCam.GetRig(i).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    public void Key_Shake_DAF(string args)
    {

        string[] split = args.Split('|');
        float[] values = new float[3];
        for (int i = 0; i < 3; ++i)
        {
            values[i] = float.Parse(split[i]);
        }
        shakeDuration = values[0];
        shakeAmplitude = values[1];
        shakeFrequency = values[2];
        //Debug.Log(values[0]);

        StartCoroutine(Shake());


    }
    private IEnumerator Shake()
    {
        for (int i = 0; i < 3; ++i)
        {
            _cinemachineBasicMultiChannelPerlin[i].m_AmplitudeGain = shakeAmplitude;
            _cinemachineBasicMultiChannelPerlin[i].m_FrequencyGain = shakeFrequency;
        }

        yield return new WaitForSeconds(shakeDuration);

        for (int i = 0; i < 3; ++i)
        {
            _cinemachineBasicMultiChannelPerlin[i].m_AmplitudeGain = 0;
            _cinemachineBasicMultiChannelPerlin[i].m_FrequencyGain = 0;
        }



        // cmFreeCam.GetRig(0).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = shakeAmplitude;
        // cmFreeCam.GetRig(1).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = shakeAmplitude;
        // cmFreeCam.GetRig(2).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = shakeAmplitude;

        // cmFreeCam.GetRig(0).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = shakeFrequency;
        // cmFreeCam.GetRig(1).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = shakeFrequency;
        // cmFreeCam.GetRig(2).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = shakeFrequency;

        // yield return new WaitForSeconds(shakeDuration);
        // cmFreeCam.GetRig(0).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 0;
        // cmFreeCam.GetRig(1).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 0;
        // cmFreeCam.GetRig(2).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 0;

        // cmFreeCam.GetRig(0).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = 0;
        // cmFreeCam.GetRig(1).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = 0;
        // cmFreeCam.GetRig(2).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = 0;

    }



}
