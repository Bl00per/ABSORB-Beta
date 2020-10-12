using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioZoneController : MonoBehaviour
{
    public AudioSource AmbientElectricity;
    public AudioSource AmbientWind;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        AmbientElectricity.enabled = true;
        AmbientWind.enabled = true;
    }
    private void OnTriggerExit(Collider other)
    {
        AmbientElectricity.enabled = false;
        AmbientWind.enabled = true;
    }
}
