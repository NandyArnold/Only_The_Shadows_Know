using UnityEngine;

public class SpawnState : EnemyAIState
{
    private float _spawnWaitTime = 2.0f; // How long to wait after spawning
    private float _timer;

    public override void Enter(EnemyAI enemyAI)
    {
      Debug.Log("Entering Spawn State");
        _timer = 0f;
        // The enemy is idle while "spawning in"
        enemyAI.Navigator.Stop();
        enemyAI.AnimController.SetSpeed(0f);
    }

    public override void Execute(EnemyAI enemyAI)
    {
        _timer += Time.deltaTime;
        if (_timer >= _spawnWaitTime)
        {
            // Check which state we should transition to.
            if (enemyAI.InitialState == InitialAIState.Guard)
            {
                enemyAI.TransitionToState(new GuardState());
            }
            else // Default to Patrol
            {
                enemyAI.TransitionToState(new PatrolState(enemyAI.PatrolRoute));
            }
        }
    }


    public override void Exit(EnemyAI enemyAI) { }
}