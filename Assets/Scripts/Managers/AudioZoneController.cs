using UnityEngine;

public class AudioZoneController : MonoBehaviour
{
    public AudioSource AmbientWind;

    private void OnTriggerEnter(Collider other)
    {
 
        AmbientWind.enabled = true;
    }
    private void OnTriggerExit(Collider other)
    {
     
        AmbientWind.enabled = true;
    }
}
