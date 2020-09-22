using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupEnemyHandler : MonoBehaviour
{
    // List of every minion within the grioup
    [SerializeField] List<EnemyHandler> minions;
    [SerializeField] List<EnemyHandler> specials;
    [SerializeField] List<EnemyHandler> elites;

    // A type to define the groups behaviour
    private enum GroupType
    {
        NONE,
        MINION,
        SPECIAL,
        ELITE,
    }

    // Adds the enemies AIBrain to the group list
    public void AddEnemy(EnemyHandler enemyHandler)
    {
    }

    // Removes the enemies AIBrain from the group list
    public void RemoveEnemy(EnemyHandler enemyHandler)
    {
    }
}
