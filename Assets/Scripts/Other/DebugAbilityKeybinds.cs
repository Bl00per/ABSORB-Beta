using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugAbilityKeybinds : MonoBehaviour
{

    private AbilityHandler _abilityHandler;

    // Start is called before the first frame update
    void Awake()
    {
        _abilityHandler = this.GetComponent<AbilityHandler>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            _abilityHandler.SetAbility(AbilityHandler.AbilityType.HAMMER);
            Debug.Log("Set ability to -> HAMMER.");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            _abilityHandler.SetAbility(AbilityHandler.AbilityType.SICKLE);
            Debug.Log("Set ability to -> SICKLE.");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            _abilityHandler.SetAbility(AbilityHandler.AbilityType.POT);
            Debug.Log("Set ability to -> POT.");
        }
    }
}
