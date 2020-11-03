using System.Collections;
using UnityEngine;
using XInputDotNetPure;
public class ControllorVibration : MonoBehaviour
{
    public static PlayerIndex _playerIndex;
    public static bool controllorVibration = true;

    static private InputManager _inputManager;

    void Start()
    {
        _inputManager = FindObjectOfType<InputManager>();
    }


    public void VibrationCheck(bool check)
    {
        controllorVibration =! controllorVibration; 
    }

    public static IEnumerator Vibrate(float leftMotor, float rightMotor, float Time)
    {
        if (_inputManager.GetIsUsingController())
        {
            GamePad.SetVibration(_playerIndex, leftMotor, rightMotor);

            yield return new WaitForSeconds(Time);

            GamePad.SetVibration(_playerIndex, 0, 0);
        }
    }

    // Call: StartCoroutine(ControllorVibration.Vibrate(.2f, .2f, .3f));

}
