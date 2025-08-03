// ArrowProjectile.cs - UPGRADED with Physics
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class ArrowProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 30f;
    [SerializeField] private float damage = 15f;
    [SerializeField] private float lifeTime = 5f;

    private Rigidbody _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
       
    }

    private void Start()
    {
        // Set the initial velocity of the arrow.
        _rigidbody.linearVelocity = transform.forward * speed;
        Destroy(gameObject, lifeTime);
    }

    // Update is no longer needed, as physics handles the movement.

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<EnemyHealth>(out EnemyHealth enemyHealth))
        {
            enemyHealth.TakeDamage(damage, GameManager.Instance.Player.gameObject);
        }

        // To make the arrow stick in the target, we can disable it instead of destroying.
        // For now, Destroy is fine.
        Destroy(gameObject);
    }
}