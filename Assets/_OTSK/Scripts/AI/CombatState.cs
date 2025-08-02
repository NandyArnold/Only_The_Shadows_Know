// CombatState.cs
using UnityEngine;

public class CombatState : EnemyAIState
{
    public override void Enter(EnemyAI enemyAI)
    {
        Debug.Log("Entering Combat State");
        CombatManager.Instance.EnterCombat(); // Notify the global manager
        enemyAI.Navigator.SetSpeed(enemyAI.Config.chaseSpeed);
    }

    public override void Execute(EnemyAI enemyAI)
    {
        bool canSeePlayer = enemyAI.Detector.CanSeePlayer();
        float distanceToPlayer = Vector3.Distance(enemyAI.transform.position, enemyAI.PlayerTarget.position);

        // Condition to STAY in combat: we can see the player OR they are within our lock-on range.
        if (canSeePlayer || distanceToPlayer <= enemyAI.Config.combatLockRange)
        {
            if (canSeePlayer)
            {
                // If we see them, always update their last known position.
                enemyAI.LastKnownPlayerPosition = enemyAI.PlayerTarget.position;
            }
            // Chase them.
            // --- NEW DYNAMIC SPEED LOGIC ---
            float combatWalkThreshold = 7f; // Enemy will walk if player is closer than 7 units
            float newSpeed;

            if (distanceToPlayer > combatWalkThreshold)
            {
                newSpeed = enemyAI.Config.chaseSpeed; // Player is far, so run
            }
            else
            {
                newSpeed = enemyAI.Config.walkCombatSpeed; // Player is close, so walk
            }

            enemyAI.Navigator.SetSpeed(newSpeed);
            enemyAI.AnimController.SetSpeed(newSpeed);
            enemyAI.Navigator.MoveTo(enemyAI.LastKnownPlayerPosition);

            // TODO: Add attack logic and the "can't attack" alarm timer here.
        }
        else
        {
            enemyAI.AnimController.SetSpeed(0); // Go to idle while transitioning
            // If we've lost them completely, transition to Alert to investigate their last spot.
            enemyAI.TransitionToState(new AlertState(enemyAI.LastKnownPlayerPosition));
        }
    }

    public override void Exit(EnemyAI enemyAI)
    {
        Debug.Log("Exiting Combat State");
        // NOTE: We don't call CombatManager.ExitCombat() here.
        // The manager will decide when combat is truly over (e.g., when ALL enemies are out of combat).
        enemyAI.Navigator.Stop();
    }
}