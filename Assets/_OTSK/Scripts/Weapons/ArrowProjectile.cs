using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))] // Changed to BoxCollider as requested
public class ArrowProjectile : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float normalSpeed = 30f; // Renamed for clarity
    [SerializeField] private float focusedSpeed = 50f;
    [SerializeField] private float damage = 15f;
    [SerializeField] private float lifeTime = 5f;

    private Rigidbody _rigidbody;
    private float _speedToUse;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        GetComponent<BoxCollider>().isTrigger = true; // Use trigger for enemy detection
    }

    private void Start()
    {
        _rigidbody.linearVelocity = transform.forward * _speedToUse;
        Destroy(gameObject, lifeTime);
    }

    /// <summary>
    /// This method is called by the BowSO immediately after instantiation.
    /// </summary>
    public void Initialize(bool isShotFocused)
    {
        // The arrow sets its own speed based on the context provided by the bow.
        _speedToUse = isShotFocused ? focusedSpeed : normalSpeed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<EnemyHealth>(out EnemyHealth enemyHealth))
        {
            enemyHealth.TakeDamage(damage, GameManager.Instance.Player.gameObject);
        }

        // As per our revert, destroy on impact.
        Destroy(gameObject);
    }
}