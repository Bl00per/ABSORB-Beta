using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrels : MonoBehaviour
{
    private Collider _collider;
    private AudioSource _audio;
    private void Start()
    {
        _audio = GetComponent<AudioSource>();
        _collider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        _audio.pitch = Random.Range(0.6f, 0.9f);
        _audio.PlayOneShot(_audio.clip);
        StartCoroutine(BarrelCD());
    }

    public IEnumerator BarrelCD()
    {
        _collider.enabled = false;
        yield return new WaitForSeconds(0.05f);
        _collider.enabled = true;
    }
}
