// In ArrowProjectile.cs
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class ArrowProjectile : MonoBehaviour
{
    [SerializeField] private float lifeTime = 5f;

    private Rigidbody _rb;
    private bool _hasHit = false;
    private GameObject _owner;
    private List<DamageInstance> _damageProfile; // Will be set by the BowSO

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        GetComponent<BoxCollider>().isTrigger = false;
        _rb.useGravity = true;
    }

    // The Initialize method now accepts the damage profile from the bow
    public void Initialize(GameObject owner, List<DamageInstance> damageProfile, float speed)
    {
        _owner = owner;
        _damageProfile = damageProfile;
        _rb.linearVelocity = transform.forward * speed;
        Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_hasHit) return;
        _hasHit = true;

        // Stop physics movement
        _rb.isKinematic = true;
        _rb.linearVelocity = Vector3.zero;
        transform.SetParent(collision.transform);

        if (collision.gameObject.TryGetComponent<EnemyHealth>(out EnemyHealth enemyHealth))
        {
            // Pass the damage profile and the stored owner.
            enemyHealth.TakeDamage(_damageProfile, _owner);
        }
    }
}