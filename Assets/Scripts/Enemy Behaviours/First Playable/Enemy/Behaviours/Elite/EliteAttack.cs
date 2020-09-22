using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EliteAttack : AIBehaviour
{
    [Header("Properties")]
    public float turnSpeed = 0.20f;
    public float justFiredProjectileTimer = 1.0f;
    public float projectileSpeed = 150.0f;
    public float projectileLifeTime = 4.0f;
    public float projectileDamage = 10.0f;
    public float dashCancelRange = 10.0f;
    //-private float _timer = 0.0f;
    //private bool _justFired = false;

    [Header("References")]
    public GameObject projectilePrefab;
    public Transform projectileStartPoint;
    public ParticleSystem waterFireEffect;

    public override void OnStateEnter() 
    {
        waterFireEffect.Play();
        EliteProjectile eliteProjectile = Instantiate(projectilePrefab, null).GetComponent<EliteProjectile>();
        eliteProjectile.InitialiseProjectile(enemyHandler, brain.PlayerTransform, projectileStartPoint, projectileSpeed, projectileLifeTime, projectileDamage);
        StartCoroutine(JustFiredTimer());
    }

    public override void OnStateExit() {}

    public override void OnStateFixedUpdate() {}

    public override void OnStateUpdate() 
    {
        // // If player gets too close when preparing to fire, the enemy will cancel the attack.
        // if (brain.GetDistanceToPlayer() < dashCancelRange)
        // {
        //     brain.SetBehaviour("Movement");
        //     return;
        // }

        // Rotate to face direction
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(brain.GetDirectionToPlayer()), turnSpeed);

        
    }

    public IEnumerator JustFiredTimer()
    {
        yield return new WaitForSecondsRealtime(justFiredProjectileTimer);
        brain.SetBehaviour("Movement");
    }
}