using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnOffTestTubeSpawner : MonoBehaviour
{
    private ObjectPooler _objectPooler;
    private AbilityHandler _abilityHandler;

    private void Awake()
    {
        _objectPooler = this.GetComponent<ObjectPooler>();
        _abilityHandler = _objectPooler.playerTransform.GetComponent<AbilityHandler>();
    }

    private void Update()
    {
        if(_abilityHandler.GetCurrentAbilityType() == AbilityHandler.AbilityType.NONE)
            _objectPooler.spawnerActive = true;
        else
            _objectPooler.spawnerActive = false;
    }
}
