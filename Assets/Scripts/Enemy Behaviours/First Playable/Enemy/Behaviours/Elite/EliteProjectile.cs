using System.Collections;
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
    public Vector3 minOrbSize;
    public Vector3 maxOrbSize;
    public float effectTime = 2.0f;
    public Transform projectileStartPoint;
    public Collider _collider;
    public Renderer _renderer;

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
        _collider.enabled = false;
        _renderer.enabled = false;
    }

    // Sets up the direction for the projectile
    public void InitialiseProjectile(EnemyHandler enemyHandler, Transform playerTransform, Transform projectileStartPoint, float speed, float lifeTime, float damage)
    {
        _collider.enabled = true;
        _renderer.enabled = true;
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
        if (_enemyHandler != null && !_enemyHandler.IsAlive())
        {
            this.transform.localScale = minOrbSize;
            waterHitEffectGO.transform.position = this.transform.position;
            waterHitEffectGO.transform.SetParent(this.gameObject.transform);
            waterHitEffectGO.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            _renderer.enabled = true;
            this.gameObject.SetActive(false);
        }


        if (this.transform.localScale.x <= maxOrbSize.x)
            this.transform.localScale += this.transform.localScale * Time.deltaTime * 2;

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

        }

        _isActive = false;
        _collider.enabled = false;
        _renderer.enabled = false;
        waterHitEffectGO.transform.SetParent(null);
        waterHitEffect.Play();
        waterHitAudio.Play();
        StartCoroutine(Cleanup());
    }

    public EnemyHandler GetHandler()
    {
        return _enemyHandler;
    }
    public IEnumerator Cleanup()
    {
        yield return new WaitForSeconds(effectTime);

        this.transform.localScale = minOrbSize;
        waterHitEffectGO.transform.SetParent(this.gameObject.transform);
        waterHitEffectGO.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        waterHitEffectGO.transform.position = this.transform.position;
        _renderer.enabled = true;
        this.gameObject.SetActive(false);
    }

    public IEnumerator LifeTimer()
    {
        yield return new WaitForSeconds(_lifeTime);

        this.transform.localScale = minOrbSize;
        waterHitEffectGO.transform.position = this.transform.position;
        waterHitEffectGO.transform.SetParent(this.gameObject.transform);
        waterHitEffectGO.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        _renderer.enabled = true;
        this.gameObject.SetActive(false);
    }


}
