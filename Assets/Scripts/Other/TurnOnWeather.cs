using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnOnWeather : MonoBehaviour
{
    public GameObject[] weather;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            foreach (GameObject go in weather)
            {
                go.SetActive(true);
            }
        }
    }
}
