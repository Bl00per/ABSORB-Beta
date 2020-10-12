using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestoryVinesTrigger : MonoBehaviour
{
    public void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.CompareTag("Vines") && collider.gameObject.layer == LayerMask.NameToLayer("Interactable"))
            collider.gameObject.SetActive(false);
    }
}
