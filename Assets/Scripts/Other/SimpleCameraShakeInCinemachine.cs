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
    private CameraManager _cameraManager;

    private CinemachineBasicMultiChannelPerlin[] _controllerBMCP;
    private CinemachineBasicMultiChannelPerlin[] _mouseBMCP;

    private void Awake()
    {
    }

    private void Start()
    {
        _cameraManager = FindObjectOfType<CameraManager>();
        
        // Creating an array to store the rigs in, to prevent using get component every Shake()
        _controllerBMCP = new CinemachineBasicMultiChannelPerlin[3];
        _mouseBMCP = new CinemachineBasicMultiChannelPerlin[3];
        for (int i = 0; i < 3; ++i)
        {
            _controllerBMCP[i] = _cameraManager.GetControllerCamera().GetRig(i).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            _mouseBMCP[i] = _cameraManager.GetMouseCamera().GetRig(i).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }
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
            if (_cameraManager.inputManager.GetIsUsingController())
            {
                _controllerBMCP[i].m_AmplitudeGain = shakeAmplitude;
                _controllerBMCP[i].m_FrequencyGain = shakeFrequency;
            }
            else
            {
                _mouseBMCP[i].m_AmplitudeGain = shakeAmplitude;
                _mouseBMCP[i].m_FrequencyGain = shakeFrequency;
            }
        }

        yield return new WaitForSeconds(shakeDuration);

        for (int i = 0; i < 3; ++i)
        {
            if (_cameraManager.inputManager.GetIsUsingController())
            {
                _controllerBMCP[i].m_AmplitudeGain = 0;
                _controllerBMCP[i].m_FrequencyGain = 0;
            }
            else
            {
                _mouseBMCP[i].m_AmplitudeGain = 0;
                _mouseBMCP[i].m_FrequencyGain = 0;
            }
        }



        // _currentFreeCam.GetRig(0).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = shakeAmplitude;
        // _currentFreeCam.GetRig(1).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = shakeAmplitude;
        // _currentFreeCam.GetRig(2).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = shakeAmplitude;

        // _currentFreeCam.GetRig(0).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = shakeFrequency;
        // _currentFreeCam.GetRig(1).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = shakeFrequency;
        // _currentFreeCam.GetRig(2).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = shakeFrequency;

        // yield return new WaitForSeconds(shakeDuration);
        // _currentFreeCam.GetRig(0).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 0;
        // _currentFreeCam.GetRig(1).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 0;
        // _currentFreeCam.GetRig(2).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 0;

        // _currentFreeCam.GetRig(0).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = 0;
        // _currentFreeCam.GetRig(1).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = 0;
        // _currentFreeCam.GetRig(2).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = 0;

    }



}
