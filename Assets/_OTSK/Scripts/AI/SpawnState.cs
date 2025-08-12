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
        // Wait for the timer to finish
        _timer += Time.deltaTime;
        if (_timer >= _spawnWaitTime)
        {
            Debug.Log("Spawn State complete, transitioning to Patrol State");
            // After the wait, transition to the normal patrol state.
            enemyAI.TransitionToState(new PatrolState(enemyAI.PatrolRoute));
        }
    }

    public override void Exit(EnemyAI enemyAI) { }
}