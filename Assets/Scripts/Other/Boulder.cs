using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boulder : MonoBehaviour
{
    public Rigidbody[] shardRigidbodies;
    private Collider _collider;

    public void Awake()
    {
        _collider = this.GetComponent<Collider>();
        ToggleKinematicOnShards(true);
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.CompareTag("Ability"))
        {
            ToggleKinematicOnShards(false);
            _collider.enabled = false;
        }
    }

    private void ToggleKinematicOnShards(bool value)
    {
        foreach (Rigidbody rb in shardRigidbodies)
            rb.isKinematic = value;
    }
}
