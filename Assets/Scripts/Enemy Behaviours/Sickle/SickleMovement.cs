using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SickleMovement : AIBehaviour
{

    [Header("Special Properties")]
    public float specialEnterAttackStateDistance = 10.0f;
    public float specialAfterAttackSpeed = 7.5f;

    [Header("Elite Properties")]
    public float eliteStartRetreatDistance = 10.0f;
    [Range(0, 1)]
    public float eliteAttackChance = 1.0f;

    [Header("Timers")]
    public float returnToInitialSpeed = 1.0f;
    public float retreatFromPlayerTimer = 0.0f;
    public float returnToInitialAngularSpeed = 1.0f;

    private float _initialSpeed = 0.0f;
    private float _initialAngularSpeed = 0.0f;
    private bool _startedRetreat = false;
    private CombatHandler _combatHandler;
    private SickleAttack _sickleAttack;

    private void Start()
    {
        // Storing the initial angular speed of the agent
        _initialSpeed = brain.GetNavMeshAgent().speed;

        _combatHandler = brain.PlayerTransform.GetComponent<CombatHandler>();
        _sickleAttack = GetComponent<SickleAttack>();
    }

    public override void OnStateEnter()
    {
        brain.GetNavMeshAgent().speed = _initialSpeed;

        if (brain.GetLastStateID() == "Attack" && enemyHandler.GetPlayerHandler().GetIsAlive())
            this.LockDestinationToPlayer(1.0f);
    }

    public override void OnStateUpdate()
    {
        // Checking if we should be locked onto the player or not...
        if (this.destinationLockedToPlayer)
            this.currentDestination = brain.PlayerTransform.position;

        // Updating the target destination every frame
        brain.SetDestinationOnCooldown(this.currentDestination, 1.0f);

        switch (enemyHandler.GetEnemyType())
        {
            case EnemyHandler.EnemyType.SPECIAL:
                UpdateSpecial();
                break;

            case EnemyHandler.EnemyType.ELITE:
                UpdateElite();
                break;
        }
    }

    public override void OnStateExit() { }

    public override void OnStateFixedUpdate() { }

    private IEnumerator ChangeSpeedAfterAttack()
    {
        _startedRetreat = true;
        brain.GetNavMeshAgent().speed = specialAfterAttackSpeed;
        yield return new WaitForSeconds(returnToInitialSpeed);
        brain.GetNavMeshAgent().speed = _initialSpeed;
        _startedRetreat = false;
    }

    private void UpdateSpecial()
    {
        // Storing the distance to preform multiple checks on
        float distance = brain.GetDistanceToPlayer();

        // Check if the enemy has just attacked, if they have then return to their previous position
        if (enemyHandler.GetJustAttacked())
        {
            if (!_startedRetreat)
                StartCoroutine(ChangeSpeedAfterAttack());
        }

        // If the remaining distance is less than or equal to the stopping distance; enter the attack behaviour.
        else if (distance <= specialEnterAttackStateDistance)
        {
            brain.SetBehaviour("Attack");
        }
    }

    private void UpdateElite()
    {
        // Storing the distance to preform multiple checks on
        float distance = brain.GetDistanceToPlayer();

        if (distance <= eliteStartRetreatDistance || enemyHandler.GetJustAttacked())
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(brain.GetDirectionToPlayer()), 0.20F);
            if (!_startedRetreat)
            {
                Vector3 retreatPosition = transform.position - (-brain.GetDirectionToPlayer() * GetRetreatDistance());
                StartCoroutine(Retreat(retreatPosition, retreatFromPlayerTimer, returnToInitialAngularSpeed));
            }
            return;
        }
        else if (_combatHandler.GetJustUsedMechanic())
        {
            if (Random.value <= eliteAttackChance)
            {
                brain.SetBehaviour("Attack");
                enemyHandler.GetEnemyGroupHandler()?.ForceAllEnemiesToRetreat(enemyHandler);
            }
        }
    }


    private IEnumerator Retreat(Vector3 position, float returnToPositionTimer, float returnToInitialAngularSpeedTimer)
    {
        _startedRetreat = true;
        brain.GetNavMeshAgent().angularSpeed = 0.0f;
        yield return new WaitForSeconds(returnToPositionTimer);
        OverrideDestination(position); ;
        yield return new WaitForSeconds(returnToInitialAngularSpeedTimer);
        brain.GetNavMeshAgent().angularSpeed = _initialAngularSpeed;
        _startedRetreat = false;
    }

    public float GetRetreatDistance()
    {
        return brain.GetDistanceToPlayer() - _sickleAttack.eliteStartDashDistance;
    }
}
