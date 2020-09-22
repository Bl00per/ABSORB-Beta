using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    public Renderer emmision;
    public float minLightRange = 5f;
    [Range(0.0f, 100.0f)]
    public float frequencyPercentage = 70.0f;
    public float flickerTime = 0.3f;
    public float maxTimeBetweenFlickers = 0.5f;

    private Light _lightObject;
    //private bool _flicker = true;
    private float _maxRange;
    private float _randNumber;
    private float _tempTimer;

    // Start is called before the first frame update
    void Start()
    {
        _lightObject = this.GetComponent<Light>();
        _maxRange = _lightObject.range;
    }

    // Update is called once per frame
    void Update()
    {
        _tempTimer += Time.deltaTime;

        if (_tempTimer >= maxTimeBetweenFlickers)
        {
            // Get a random number
            _randNumber = CheckForFlicker();
            // if that random number is less than the frequency then flicker
            if (_randNumber <= frequencyPercentage)
            {
                _lightObject.range = minLightRange;
                emmision.material.DisableKeyword("_EMISSION");
                StartCoroutine(Flicker());
            }
        }
    }

    private IEnumerator Flicker()
    {
        yield return new WaitForSeconds(flickerTime);
        _tempTimer = 0f;
        emmision.material.EnableKeyword("_EMISSION");
        _lightObject.range = _maxRange;
    }

    private float CheckForFlicker()
    {
        return Random.Range(0.0f, 100.0f);
    }

    public void Disable()
    {
        StopCoroutine(Flicker());
        _lightObject.range = minLightRange;
        emmision.material.DisableKeyword("_EMISSION");
        this.enabled = false;
    }
}
