using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempBoulderTrigger : MonoBehaviour
{
    public BoxCollider boxCollider;
    public Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        boxCollider.enabled = true;
    }

    void OnTriggerEnter(Collider other)
    {
        // if (other.gameObject.layer == LayerMask.NameToLayer("PlayerWeapon"))
        // {
        //     animator.SetBool("DoorTriggered", true);
        //     boxCollider.enabled = false;
        // }
    }
}
