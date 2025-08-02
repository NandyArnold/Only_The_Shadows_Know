// Enemy.cs - UPGRADED to handle death
using System.Collections;
using System.Xml;
using UnityEngine;
using UnityEngine.AI;

// We add [RequireComponent] to ensure a UniqueID is always present.
[RequireComponent(typeof(UniqueID))]
public class Enemy : MonoBehaviour, ISaveable
{

    // --- Save Data Structure ---
    [System.Serializable]
    private struct EnemySaveData
    {
        public Vector3 position;
        public Quaternion rotation;
        public float currentHealth;
    }
    // --- Component References 
    [SerializeField] private EnemyConfigSO config;
    public EnemyConfigSO Config => config;
    public DetectionSystem Detector { get; private set; }

    // Component references
    private EnemyHealth _health;
    private EnemyNavigator _navigator;
    private EnemyAI _ai;
    private EnemyAnimationController _animController;
    private CapsuleCollider _collider;
    private UniqueID _uniqueID;


    // --- ISaveable Implementation ---
    public string UniqueID => _uniqueID.ID;

    public object CaptureState()
    {
        return new EnemySaveData
        {
            position = transform.position,
            rotation = transform.rotation,
            currentHealth = _health.CurrentHealth // We need to add this property to EnemyHealth
        };
    }

    public void RestoreState(object state)
    {
        var saveData = (EnemySaveData)state;
        transform.position = saveData.position;
        transform.rotation = saveData.rotation;
        _health.SetHealth(saveData.currentHealth);
    }
    private void Awake()
    {
        Detector = GetComponent<DetectionSystem>();
        _health = GetComponent<EnemyHealth>();
        _navigator = GetComponent<EnemyNavigator>();
        _ai = GetComponent<EnemyAI>();
        _animController = GetComponent<EnemyAnimationController>();
        _collider = GetComponent<CapsuleCollider>();
        _uniqueID = GetComponent<UniqueID>();
    }

    private void OnEnable()
    {
        EnemyManager.Instance.RegisterEnemy(this);
        _health.OnDied += HandleDeath; // Subscribe to the death event
    }

    private void OnDisable()
    {
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.UnregisterEnemy(this);
        }
        _health.OnDied -= HandleDeath; // Unsubscribe
    }

    private void Start()
    {
        _navigator.SetSpeed(config.patrolSpeed);
    }

    private void HandleDeath()
    {
        // 1. Disable all intelligence and movement
        _ai.enabled = false;
        _navigator.enabled = false;
        GetComponent<NavMeshAgent>().enabled = false;
        _collider.enabled = false;

        // 2. Play the death animation
        _animController.PlayDeathAnimation();

        // 3. Start a timer to remove the body
        StartCoroutine(CleanupBody(5f)); // Wait 5 seconds before removing body
    }

    private IEnumerator CleanupBody(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}