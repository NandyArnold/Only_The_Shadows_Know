// ChaseState.cs - CORRECTED VERSION

using UnityEngine;

public class ChaseState : EnemyAIState
{
    public override void Enter(EnemyAI enemyAI)
    {
        Debug.Log("Entering Chase State");
        // We only set the speed once when entering the state.
        enemyAI.Navigator.SetSpeed(enemyAI.Config.chaseSpeed);
    }

    public override void Execute(EnemyAI enemyAI)
    {
        // This runs every frame, so the destination is constantly updated.
        if (enemyAI.PlayerTarget != null)
        {
            enemyAI.Navigator.MoveTo(enemyAI.PlayerTarget.position);
        }
    }

    public override void Exit(EnemyAI enemyAI)
    {
        Debug.Log("Exiting Chase State");
        // We can tell the navigator to stop when we exit the chase.
        enemyAI.Navigator.Stop();
    }
}