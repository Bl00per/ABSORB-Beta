using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using XInputDotNetPure;

public class ControllorVibration : MonoBehaviour
{
    public static PlayerIndex _playerIndex;
    public static bool controllorVibration = true;
    public Toggle vibrationToggle;

    static private InputManager _inputManager;

    void Awake()
    {
        _inputManager = FindObjectOfType<InputManager>();
        vibrationToggle.isOn = controllorVibration;
    }

    public void ControllorVibrationToggle()
    {
        controllorVibration = !controllorVibration; 
    }

    public static IEnumerator Vibrate(float leftMotor, float rightMotor, float Time)
    {
        if (controllorVibration && _inputManager.GetIsUsingController())
        {
            GamePad.SetVibration(_playerIndex, leftMotor, rightMotor);

            yield return new WaitForSeconds(Time);

            GamePad.SetVibration(_playerIndex, 0, 0);
        }
    }

    // Call: StartCoroutine(ControllorVibration.Vibrate(.2f, .2f, .3f));

}
