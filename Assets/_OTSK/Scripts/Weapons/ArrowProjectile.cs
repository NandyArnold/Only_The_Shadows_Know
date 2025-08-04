using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class ArrowProjectile : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float normalSpeed = 30f;
    [SerializeField] private float focusedSpeed = 50f;
    [SerializeField] private float damage = 15f;
    [SerializeField] private float lifeTime = 10f; // Increased lifetime so it can stick for a while

    private Rigidbody _rb;
    private bool _hasHit = false;
    private float _speedToUse;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        // We now need a solid collider for OnCollisionEnter, so Is Trigger is false.
        GetComponent<BoxCollider>().isTrigger = false;
    }

    public void Initialize(bool isShotFocused)
    {
        _speedToUse = isShotFocused ? focusedSpeed : normalSpeed;

        // We set the velocity immediately upon initialization
        transform.forward = transform.forward; // Ensures rotation is applied before setting velocity
        _rb.linearVelocity = transform.forward * _speedToUse;

        Destroy(gameObject, lifeTime);
    }

    // We use OnCollisionEnter for solid objects like the ground, walls, and enemies.
    private void OnCollisionEnter(Collision collision)
    {
        // Prevent the same arrow from hitting multiple things.
        if (_hasHit) return;
        _hasHit = true;

        // Stop all physics movement to "stick" the arrow.
        _rb.linearVelocity = Vector3.zero;
        _rb.isKinematic = true;

        // Parent the arrow to the object it hit.
        transform.SetParent(collision.transform);

        // Check if the object we hit was an enemy.
        if (collision.gameObject.TryGetComponent<EnemyHealth>(out EnemyHealth enemyHealth))
        {
            enemyHealth.TakeDamage(damage, GameManager.Instance.Player.gameObject);
        }
    }
}