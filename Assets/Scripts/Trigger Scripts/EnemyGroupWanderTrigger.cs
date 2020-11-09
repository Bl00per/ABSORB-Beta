using UnityEngine;

public class EnemyGroupWanderTrigger : MonoBehaviour
{
    public EnemyGroupHandler[] enemyGroups;

    public void OnTriggerEnter(Collider collider)
    {
        if(collider.gameObject.CompareTag("Player"))
        {
            foreach(EnemyGroupHandler group in enemyGroups)
            {
                group.SetState(EnemyGroupHandler.GroupState.WANDER);
            }
        }
    }
}
