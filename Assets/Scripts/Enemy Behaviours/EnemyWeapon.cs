using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWeapon : MonoBehaviour
{
    private EnemyHandler _enemyHandler;

    private void Awake()
    {
        _enemyHandler = this.GetComponentInParent<EnemyHandler>();
    }

    public EnemyHandler GetEnemyHandler() => _enemyHandler;

    public void SetEnemyHandler(EnemyHandler enemyHandler) => _enemyHandler = enemyHandler;
}
