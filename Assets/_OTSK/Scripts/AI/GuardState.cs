// Create this new script, GuardState.cs
using UnityEngine;

public class GuardState : EnemyAIState
{


    public override void Enter(EnemyAI enemyAI)
    {
        Debug.Log("Entering Guard State");
        // Tell the navigator to go back to its spawn position.
        enemyAI.Navigator.Resume();
        enemyAI.Navigator.SetSpeed(enemyAI.Config.patrolSpeed);
        enemyAI.AnimController.SetSpeed(enemyAI.Config.patrolSpeed);
        enemyAI.Navigator.MoveTo(enemyAI.SpawnPosition);
    }

    public override void Execute(EnemyAI enemyAI)
    {
        if (!enemyAI.Navigator.HasReachedDestination)
        {
            return;
        }


        // If we have arrived at our guard post, stop and go idle.
        if (enemyAI.Navigator.HasReachedDestination)
        {
            enemyAI.Navigator.Stop();
            enemyAI.AnimController.SetSpeed(0f);
        }

        // A guarding enemy still needs to be able to see the player.
        // This is the same logic from PatrolState.
        if (enemyAI.PlayerTarget != null && enemyAI.Detector.CanSeePlayer())
        {
            float distanceToPlayer = Vector3.Distance(enemyAI.transform.position, enemyAI.PlayerTarget.position);
            if (distanceToPlayer <= enemyAI.Config.combatEntryRange)
            {
                enemyAI.TransitionToState(new CombatState());
            }
            else
            {
                enemyAI.TransitionToState(new AlertState(enemyAI.PlayerTarget.position));
            }
        }
    }

    public override void Exit(EnemyAI enemyAI)
    {
        enemyAI.Navigator.Resume();
    }
}
