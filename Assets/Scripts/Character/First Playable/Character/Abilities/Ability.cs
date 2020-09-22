using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Ability : MonoBehaviour
{
    protected AbilityHandler abilityHandler;
    public void Initialise(AbilityHandler abilityHandler)
    {
        this.abilityHandler = abilityHandler;
    }

    [Header("Damage Properties")]
    public float damageToMinion = 50.0f;
    public float damageToSpecial = 20.0f;
    public float damageToElite = 100.0f;
    protected bool active = false;
    public abstract void OnEnter();
    public abstract void OnExit();
    public abstract void Activate();
    public bool IsActive() => active;
}
