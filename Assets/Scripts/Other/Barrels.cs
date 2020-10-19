using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrels : MonoBehaviour
{

    private AudioSource _audio;
    private void Start()
    {
        _audio = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        _audio.pitch = Random.Range(0.6f, 0.9f);
        _audio.PlayOneShot(_audio.clip);
    }


}
