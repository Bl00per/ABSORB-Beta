using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    //public Trigger triggerBox;
    [Header("References")]
    public Transform spawnPosition;
    public EnemyGroupHandler[] enemyGroupHandler;
    public Transform enemyRetreatToPosition;
    private PlayerHandler _playerHandler;

    // Start is called before the first frame update
    void Start()
    {
        _playerHandler = FindObjectOfType<PlayerHandler>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _playerHandler.SetRespawnPosition(spawnPosition.position);

            foreach(EnemyGroupHandler group in enemyGroupHandler)
            {
                if(group.GetCurrentState() == EnemyGroupHandler.GroupState.COMBAT || group.GetCurrentState() == EnemyGroupHandler.GroupState.CHASE)
                {
                    group.SetState(EnemyGroupHandler.GroupState.RETREAT);
                    group.SetAllEnemiesIdle();
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(this.spawnPosition.position, 1f);
    }
}
