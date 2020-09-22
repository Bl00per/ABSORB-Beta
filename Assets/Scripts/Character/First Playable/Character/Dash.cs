using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dash : MonoBehaviour
{
    [Header("Properties")]
    public float force = 50.0f;
    public float cooldownTime = 5.0f;
    public float distance = 20.0f;
    private bool _canDash = true;
    private Rigidbody _rigidbody;
    private SpecialParryBlock _specialParry;
    private InputManager _inputManager;
    private Vector3 _initialVelocity = Vector3.zero;
    private Vector3 _initialPosition = Vector3.zero;
    private bool _haveReset = false;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _specialParry = GetComponent<SpecialParryBlock>();
        _inputManager = FindObjectOfType<InputManager>();
    }

    private void Update()
    {
        if (_inputManager.GetDashButtonPress() && _canDash && _specialParry.shieldState != SpecialParryBlock.ShieldState.Shielding)
        {
            _initialVelocity = _rigidbody.velocity;
            _initialPosition = transform.position;
            _rigidbody.AddForce(transform.forward * force, ForceMode.Impulse);
            _canDash = false;
            StartCoroutine(CoolDownSequence());
        }

        if (!_canDash)
        {
            if (Vector3.Distance(transform.position, _initialPosition) > distance && !_haveReset)
            {
                _rigidbody.velocity = _initialVelocity;
                _initialVelocity = Vector3.zero;
                _initialPosition = Vector3.zero;
                _haveReset = true;
            }
        }
    }

    private IEnumerator CoolDownSequence()
    {
        yield return new WaitForSecondsRealtime(cooldownTime);
        _canDash = true;
        _haveReset = false;
        Debug.Log("Dash ready...");
    }
}
