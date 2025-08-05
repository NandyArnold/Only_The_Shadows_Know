using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class ArrowProjectile : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField]private DamageTypeSO damageType;

    [SerializeField] private float damage = 15f;
    [SerializeField] private float lifeTime = 10f; // Increased lifetime so it can stick for a while
    [SerializeField] private List<DamageInstance> damageProfile;

    private Rigidbody _rb;
    private bool _hasHit = false;
    private float _initialSpeed;
    private bool _isReadyToFire = false;
    private GameObject _owner;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        // We now need a solid collider for OnCollisionEnter, so Is Trigger is false.
        GetComponent<BoxCollider>().isTrigger = false;
    }

    public void Initialize(GameObject owner, float speed)
    {
        _owner = owner;
        _initialSpeed = speed;
        _isReadyToFire = true;
        Destroy(gameObject, lifeTime);
    }
    private void FixedUpdate()
    {
        // This logic is unchanged, but now uses the speed it was given
        if (_isReadyToFire)
        {
            _rb.linearVelocity = transform.forward * _initialSpeed;
            _isReadyToFire = false;
        }
    }

    // We use OnCollisionEnter for solid objects like the ground, walls, and enemies.
    private void OnCollisionEnter(Collision collision)
    {
        if (_hasHit) return;
        _hasHit = true;

        if (collision.gameObject.TryGetComponent<EnemyHealth>(out EnemyHealth enemyHealth))
        {
            // Pass the damage profile and the stored owner.
            enemyHealth.TakeDamage(damageProfile, _owner);
        }

        // Stick the arrow into the surface.
        _rb.isKinematic = true;
        _rb.linearVelocity = Vector3.zero;
        transform.SetParent(collision.transform);
    }
}