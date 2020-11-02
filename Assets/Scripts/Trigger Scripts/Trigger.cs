using UnityEngine;

public class Trigger : MonoBehaviour
{
    private bool _enabled = false;
    private Collider _collider = null;

    private void OnTriggerEnter(Collider other)
    {
        _enabled = true;
        _collider = other;
    }

    private void OnTriggerExit(Collider other)
    {
        _enabled = false;
        _collider = null;
    }

    // private void OnTriggerStay(Collider other)
    // {
    //     _enabled = true;
    //     _collider = other;
    // }

    public bool Enabled
    {
        get { return _enabled; }
        set { _enabled = value; }
    }

    public Collider Collider
    {
        get { return _collider; }
        set { _collider = value; }
    }
}
