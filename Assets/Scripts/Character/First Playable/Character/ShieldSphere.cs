using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldSphere : MonoBehaviour
{
    public CombatHandler player;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("EnemyWeapon"))
        {
            EnemyHandler enemy = other.GetComponentInParent<EnemyHandler>();
            if (enemy.GetEnemyType() == EnemyHandler.EnemyType.SPECIAL)
            {
                enemy.GetBrain().SetBehaviour("Parried");
                return;
            }
        }
    }
}
