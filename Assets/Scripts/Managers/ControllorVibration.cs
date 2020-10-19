using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;
public class ControllorVibration : MonoBehaviour
{
    public static PlayerIndex _playerIndex;
    public static bool controllorVibration = true;


    public void VibrationCheck(bool check)
    {
        controllorVibration =! controllorVibration; 
    }

    public static IEnumerator Vibrate(float leftMotor, float rightMotor, float Time)
    {
        if (controllorVibration)
        {
            GamePad.SetVibration(_playerIndex, leftMotor, rightMotor);

            yield return new WaitForSeconds(Time);

            GamePad.SetVibration(_playerIndex, 0, 0);
        }
    }

    // Call: StartCoroutine(ControllorVibration.Vibrate(.2f, .2f, .3f));

}
