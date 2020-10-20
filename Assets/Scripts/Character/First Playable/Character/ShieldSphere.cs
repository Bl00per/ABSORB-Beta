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
            Debug.Log(other.gameObject.name);
            // Get enemy handler out of the enemy weapon
            EnemyHandler enemy = other.gameObject.GetComponent<EnemyWeapon>().GetEnemyHandler();

            enemy.PlayHitParryEffect();
            if (enemy.GetEnemyType() == EnemyHandler.EnemyType.SPECIAL)
            {
                enemy.GetBrain().SetBehaviour("Parried");
                return;
            }
        }
    }
}
