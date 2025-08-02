// Enemy.cs

using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private EnemyConfigSO config;
    public EnemyConfigSO Config => config;

    // References to other enemy components
    private EnemyHealth _health;
    private EnemyNavigator _navigator;

    private DetectionSystem _detector; 
    public DetectionSystem Detector => _detector;
    // We will add references to EnemyAI, EnemyCombatHandler, etc. here later.

    private void Awake()
    {
        _health = GetComponent<EnemyHealth>();
        _navigator = GetComponent<EnemyNavigator>();
        _navigator = GetComponent<EnemyNavigator>();
        _detector = GetComponent<DetectionSystem>();
    }


    private void Start()
    {
        // Initialize components with data from the config file
        _navigator.SetSpeed(config.patrolSpeed);
    }

    private void OnEnable()
    {
        EnemyManager.Instance.RegisterEnemy(this);
    }

    private void OnDestroy()
    {
        // Use a null check in case the manager was destroyed first on scene unload
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.UnregisterEnemy(this);
        }
    }
}
