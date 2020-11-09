using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotMovement : AIBehaviour
{
    [Header("Properties")]
    public float enterAttackStateDistance = 10.0f;
    public float retreatFromPlayerDistance = 7.0f;

    [Header("Timers")]
    public float retreatFromPlayerTimer = 0.0f;
    public float returnToInitialAngularSpeed = 1.0f;
    private float _initialAngularSpeed = 0.0f;
    private bool _startedRetreat = false;
    private bool _hasAttacked = false;
    private PotAttack _potAttack;
    private EliteProjectile _projectile;

    private void Awake()
    {
        _potAttack = this.GetComponent<PotAttack>();
    }

    private void Start()
    {
        // Storing the initial angular speed of the agent
        _initialAngularSpeed = brain.GetNavMeshAgent().angularSpeed;
        _projectile = _potAttack.GetProjectile();
    }

    public override void OnStateEnter()
    {
        brain.GetNavMeshAgent().angularSpeed = _initialAngularSpeed;
    }

    public override void OnStateUpdate()
    {
        float distance = brain.GetDistanceToPlayer();

        if (enemyHandler.GetJustAttacked() || distance <= retreatFromPlayerDistance)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(brain.GetDirectionToPlayer()), 0.20F);
            if (!_startedRetreat)
            {
                Vector3 retreatPosition = transform.position - (-brain.GetDirectionToPlayer() * GetRetreatDistance());
                StartCoroutine(Retreat(retreatPosition, retreatFromPlayerTimer, returnToInitialAngularSpeed));
            }
        }
        else if (distance <= enterAttackStateDistance && !_startedRetreat && !_projectile.GetCleanUp())
        {
            brain.SetBehaviour("Attack");
            _hasAttacked = true;

            if (enemyHandler.GetEnemyType() == EnemyHandler.EnemyType.ELITE)
                enemyHandler.GetEnemyGroupHandler()?.ForceAllEnemiesToRetreat(enemyHandler);
        }
        else if (_hasAttacked && !_startedRetreat)
        {
            if (enemyHandler.GetPlayerHandler().GetIsAlive())
            {
                if (distance > enterAttackStateDistance)
                {
                    this.LockDestinationToPlayer();
                }
            }
            else
            {
                _hasAttacked = false;
            }

        }
    }

    public override void OnStateExit() { }

    public override void OnStateFixedUpdate() { }

    private IEnumerator Retreat(Vector3 position, float returnToPositionTimer, float returnToInitialAngularSpeedTimer)
    {
        _startedRetreat = true;
        brain.GetNavMeshAgent().angularSpeed = 0.0f;
        yield return new WaitForSeconds(returnToPositionTimer);
        OverrideDestination(position);
        yield return new WaitForSeconds(returnToInitialAngularSpeedTimer);
        brain.GetNavMeshAgent().angularSpeed = _initialAngularSpeed;
        _startedRetreat = false;
    }

    public float GetRetreatDistance()
    {
        return brain.GetDistanceToPlayer() - enterAttackStateDistance;
    }
}
