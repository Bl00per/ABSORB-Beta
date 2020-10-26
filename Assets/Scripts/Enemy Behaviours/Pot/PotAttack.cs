using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotAttack : AIBehaviour
{
    [Header("Properties")]
    public float turnSpeed = 0.20f;
    public float justFiredProjectileTimer = 1.0f;
    public float projectileSpeed = 150.0f;
    public float projectileLifeTime = 4.0f;
    public float projectileDamage = 10.0f;

    [Header("Special Properties")]
    public float retreatFromPlayerTime = 2.0f;

    [Header("References")]
    public GameObject projectilePrefab;
    public Transform projectileStartPoint;
    public ParticleSystem chargingEffect;

    private Animator _animator;
    private PotMovement _potMovement;
    private bool _startedRetreatTimer = false;

    public void Awake()
    {
        _animator = this.GetComponent<Animator>();
        _potMovement = this.GetComponent<PotMovement>();
    }

    public override void OnStateEnter()
    {
        _animator.SetBool("Attacking", true);
        switch (enemyHandler.GetEnemyType())
        {
            case EnemyHandler.EnemyType.ELITE:
                //brain.SetBehaviour("Movement");
                break;

            case EnemyHandler.EnemyType.SPECIAL:
                enemyHandler.GetBrain().GetNavMeshAgent().isStopped = true;
                break;
        }


        /*        waterFireEffect.Play();
                EliteProjectile eliteProjectile = Instantiate(projectilePrefab, null).GetComponent<EliteProjectile>();
                eliteProjectile.InitialiseProjectile(enemyHandler, brain.PlayerTransform, projectileStartPoint, projectileSpeed, projectileLifeTime, projectileDamage);
                StartCoroutine(JustFiredTimer());*/
    }

    public override void OnStateExit()
    {
        _animator.SetBool("Attacking", false);
        switch (enemyHandler.GetEnemyType())
        {
            case EnemyHandler.EnemyType.ELITE:
                break;

            case EnemyHandler.EnemyType.SPECIAL:
                enemyHandler.GetBrain().GetNavMeshAgent().isStopped = false;
                break;
        }

    }

    public override void OnStateFixedUpdate() { }

    public override void OnStateUpdate()
    {
        // // If player gets too close when preparing to fire, the enemy will cancel the attack.
        // if (brain.GetDistanceToPlayer() < dashCancelRange)
        // {
        //     brain.SetBehaviour("Movement");
        //     return;
        // }

        if (brain.GetDistanceToPlayer() <= _potMovement.enterAttackStateDistance + 1.0f)
        {
            switch (enemyHandler.GetEnemyType())
            {
                case EnemyHandler.EnemyType.ELITE:
                    brain.SetBehaviour("Movement");
                    break;

                case EnemyHandler.EnemyType.SPECIAL:
                    if (!_startedRetreatTimer)
                        StartCoroutine(RetreatFromPlayer());
                    break;
            }
            return;
        }

        // Rotate to face direction
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(brain.GetDirectionToPlayer()), turnSpeed);

    }
    public IEnumerator RetreatFromPlayer()
    {
        _startedRetreatTimer = true;
        yield return new WaitForSeconds(retreatFromPlayerTime);
        brain.SetBehaviour("Movement");
        _startedRetreatTimer = false;
    }

    public void key_ProjectileCharge()
    {

        chargingEffect.Play();
        projectilePrefab.transform.position = projectileStartPoint.position;
        projectilePrefab.transform.SetParent(this.gameObject.transform);
        projectilePrefab.SetActive(true);
    }

    public void key_FireProjectile()
    {
        _animator.SetBool("Attacking", false);
        EliteProjectile eliteProjectile = projectilePrefab.GetComponent<EliteProjectile>();
        eliteProjectile.InitialiseProjectile(enemyHandler, brain.PlayerTransform, projectileStartPoint, projectileSpeed, projectileLifeTime, projectileDamage);
        projectilePrefab.transform.SetParent(null);
        StartCoroutine(JustFiredTimer());
    }

    public IEnumerator JustFiredTimer()
    {
        yield return new WaitForSecondsRealtime(justFiredProjectileTimer);
        if (!brain.GetHandler().IsParried())
            brain.SetBehaviour("Movement");
    }
}
