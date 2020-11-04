using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boulder : MonoBehaviour
{
    public float timeBeforeRBDisabled = 20.0f;
    public PlayerHandler playerHandler;
    public AbilityHandler.AbilityType triggerAbility;
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
            Ability ability = playerHandler.GetAbilityHandler().GetCurrentAbility();
            if (ability.GetAbilityType() == triggerAbility)
            {
                ToggleKinematicOnShards(false);
                _collider.enabled = false;
            }
        }
    }

    private void ToggleKinematicOnShards(bool value)
    {
        foreach (Rigidbody rb in shardRigidbodies)
            rb.isKinematic = value;
    }

    private IEnumerator DisableRB()
    {
        yield return new WaitForSeconds(timeBeforeRBDisabled);
        ToggleKinematicOnShards(true);
    }
}
