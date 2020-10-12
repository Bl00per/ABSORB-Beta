﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EliteProjectile : MonoBehaviour
{
    [Header("VFX References")]
    public GameObject waterHitEffectGO;
    public ParticleSystem waterHitEffect;
    public ParticleSystem waterParryEffect;
    public AudioSource waterHitAudio;
    public AudioSource waterParryAudio;

    [Header("Properties")]
    public Vector3 directionOffset = Vector3.zero;
    public float effectTime = 2.0f;

    private Rigidbody _rb;
    private Vector3 _direction = Vector3.zero;
    private float _speed = 0.0f;
    private bool _isActive = false;
    private float _lifeTime = 0.0f;
    private Transform _parentTransform;
    private EnemyHandler _enemyHandler;
    private EnemyWeapon enemyWeapon;
    private float _damage;

    // Gets called on awake
    private void Awake()
    {
        _rb = this.GetComponent<Rigidbody>();
        enemyWeapon = this.GetComponent<EnemyWeapon>();
    }

    // Sets up the direction for the projectile
    public void InitialiseProjectile(EnemyHandler enemyHandler, Transform playerTransform, Transform projectileStartPoint, float speed, float lifeTime, float damage)
    {
        transform.position = projectileStartPoint.position;
        transform.rotation = projectileStartPoint.rotation;
        _enemyHandler = enemyHandler;
        _parentTransform = enemyHandler.transform;
        enemyWeapon.SetEnemyHandler(enemyHandler);
        _isActive = true;

        float distance = Vector3.Distance(playerTransform.position, transform.position);

        Rigidbody rb = playerTransform.GetComponent<Rigidbody>();
        Vector3 playerPos = playerTransform.position + directionOffset;
        _direction = ((playerPos + ((rb.velocity.normalized * (speed + distance)) * Time.fixedDeltaTime)) - transform.position).normalized;

        _speed = speed;
        _lifeTime = lifeTime;
        _damage = damage;
        StartCoroutine(LifeTimer());
    }

    // Gets called every frame
    private void Update()
    {
        if (_isActive)
            _rb.MovePosition(transform.position + _direction * _speed * Time.deltaTime);
    }

    // Returns the damage of this projectile
    public float GetDamage()
    {
        return _damage;
    }

    // Returns true if the projectile is still active within the scene.
    public bool IsActive
    {
        get { return _isActive; }
        set { _isActive = value; }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (_enemyHandler.GetEnemyType() == EnemyHandler.EnemyType.SPECIAL && collision.CompareTag("PlayerShield"))
        {
            _enemyHandler.GetBrain().SetBehaviour("Parried");
            waterParryEffect.Play();
            waterParryAudio.Play();
            waterHitEffectGO.transform.SetParent(null);
        }

        _isActive = false;
        waterHitEffect.Play();
        waterHitEffect.gameObject.transform.SetParent(null);
        StartCoroutine(Cleanup());
    }

    public EnemyHandler GetHandler()
    {
        return _enemyHandler;
    }

    public IEnumerator LifeTimer()
    {
        yield return new WaitForSeconds(_lifeTime);
        Destroy(this.gameObject);
    }

    public void Cleanup()
    {
        Destroy(this.gameObject);
        Destroy(waterParryEffect.gameObject, effectTime);
        yield return new WaitForSeconds(1.5f);
        Destroy(waterHitEffectGO, effectTime);
    }
}
