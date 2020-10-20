using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWeapon : MonoBehaviour
{
    //public bool 
    public EnemyHandler _enemyHandler;

    public EnemyHandler GetEnemyHandler() => _enemyHandler;

    public void SetEnemyHandler(EnemyHandler enemyHandler) => _enemyHandler = enemyHandler;
}
