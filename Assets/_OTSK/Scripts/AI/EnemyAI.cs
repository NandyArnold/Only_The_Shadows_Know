// EnemyAI.cs
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    // Public properties for states to access components
    public EnemyConfigSO Config { get; private set; }
    public EnemyNavigator Navigator { get; private set; }

    [Header("AI Data")]
    [SerializeField] private PatrolRouteSO patrolRoute;

    private EnemyAIState _currentState;

    private void Awake()
    {
        // Get references for the states to use
        Config = GetComponent<Enemy>().Config;
        Navigator = GetComponent<EnemyNavigator>();
    }

    private void Start()
    {
        // The enemy always starts in the Patrol state.
        TransitionToState(new PatrolState(patrolRoute));
    }

    private void Update()
    {
        // Run the current state's logic every frame.
        _currentState?.Execute(this);
    }

    public void TransitionToState(EnemyAIState newState)
    {
        _currentState?.Exit(this);
        _currentState = newState;
        _currentState.Enter(this);
    }
}