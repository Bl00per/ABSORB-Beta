using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugAbilityKeybinds : MonoBehaviour
{

    private AbilityHandler _abilityHandler;
    private PlayerHandler _playerHandler;

    // Start is called before the first frame update
    void Awake()
    {
        _abilityHandler = this.GetComponent<AbilityHandler>();
        _playerHandler = this.GetComponent<PlayerHandler>();
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
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            if(_playerHandler.TakeDamage(20) > 0.0f)
            {
                Debug.Log($"Took damage on purpose! Current health -> {_playerHandler.GetCurrentHealth()}.");
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            _playerHandler.GetCombatHandler().HealOvertime(20, 5.0f);
            Debug.Log($"Started heal on purpose! Current health -> {_playerHandler.GetCurrentHealth()}");
        }
    }
}
