using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class PlayerSlowdown : MonoBehaviour
{
    float maxSlowAcceleration = 10.0f;  // Lowest speed the player will be slowed down to
    float timeToAcceleration = 0.2f;    // Approx time for the player acceleration to reach the slow/default amount


    private Movement _playerMovement;
    private float _speedSmoothVelocity;
    private float _tempPlayerAcceleration;
    private bool _resetPlayerAcceleration = false;

    public enum SlowState
    {
        Default,    // Default to chill on until another state is called
        Slowdown,   // Slow down the player
        SpeedUp     // Speed up the player
    }
    [HideInInspector]
    public SlowState slowState;

    // Start is called before the first frame update
    void Start()
    {
        _playerMovement = GetComponent<Movement>();
        _speedSmoothVelocity = _playerMovement.maxVelocity;
        _tempPlayerAcceleration = _playerMovement.acceleration;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        switch (slowState)
        {
            case SlowState.Default:
                ResetPlayer();
                break;
            case SlowState.Slowdown:
                SlowdownPlayer();
                break;
            case SlowState.SpeedUp:
                SpeedUpPlayer();
                break;
        }
    }

    // Set the amount to slowdown the player by and the time it takes to slowdown to the set acceleration
    public void SetSlowdown(float slowAccelerationAmount = 10.0f, float timeToSlowdown = 0.2f)
    {
        maxSlowAcceleration = slowAccelerationAmount;
        timeToAcceleration = timeToSlowdown;
        slowState = SlowState.Slowdown;
        return;
    }

    // Set the amount of time for the player to return to the original acceleration
    public float SetSpeedUp(float timeToSpeedUp = 0.2f)
    {
        timeToAcceleration = timeToSpeedUp;
        slowState = SlowState.SpeedUp;
        return timeToAcceleration;
    }

    // Reset the player acceleration once as a failsafe
    private void ResetPlayer()
    {
        if (_resetPlayerAcceleration)
        {
            _playerMovement.acceleration = _tempPlayerAcceleration;
            _resetPlayerAcceleration = false;
        }
    }

    private void SlowdownPlayer()
    {
        _playerMovement.acceleration = Mathf.SmoothDamp(_playerMovement.acceleration, maxSlowAcceleration, ref _speedSmoothVelocity, timeToAcceleration);
    }

    private void SpeedUpPlayer()
    {
        _playerMovement.acceleration = Mathf.SmoothDamp(_playerMovement.acceleration, _tempPlayerAcceleration, ref _speedSmoothVelocity, timeToAcceleration);
        _resetPlayerAcceleration = true;

        // Failsafe to set back to default state so the slowdown state can be called again
        // It just makes sense that it goes back to normal once you reach normal speed again
        if (_playerMovement.acceleration >= _tempPlayerAcceleration)
            slowState = SlowState.Default;
    }

}
