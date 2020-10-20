using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakBoulderWithHammer : MonoBehaviour
{
    public void OnTriggerEnter(Collider collider)
    {
        if(collider.gameObject.CompareTag("Ability"))
        {
            this.gameObject.SetActive(false);
        }
    }
}
