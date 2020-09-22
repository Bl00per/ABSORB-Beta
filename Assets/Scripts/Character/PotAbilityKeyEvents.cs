using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotAbilityKeyEvents : MonoBehaviour
{
    public AbilityPot abilityPot;

    public void KeyEvent_DeactivateAbility()
    {
        abilityPot.Key_DeactivatePotAbility();
    }

    public void KeyEvent_ActivateVFX()
    {
        abilityPot.Key_ActivateOrbHitVFX();
    }

}
