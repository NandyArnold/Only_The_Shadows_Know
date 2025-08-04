// CombatState.cs - CORRECTED CHASE LOGIC

using UnityEngine;

public class CombatState : EnemyAIState
{
    public override void Enter(EnemyAI enemyAI)
    {
        Debug.Log("Entering Combat State");
        CombatManager.Instance.EnterCombat();
        enemyAI.Navigator.SetSpeed(enemyAI.Config.chaseSpeed);
        enemyAI.AnimController.SetSpeed(enemyAI.Config.chaseSpeed);
    }

    public override void Execute(EnemyAI enemyAI)
    {
        if (enemyAI.PlayerTarget == null) // If the player is no longer available, exit combat.
        {
            // Go back to patrolling since there's nothing to chase.
            enemyAI.TransitionToState(new PatrolState(enemyAI.PatrolRoute));
            return;
        }

        bool canSeePlayer = enemyAI.Detector.CanSeePlayer();
        float distanceToPlayer = Vector3.Distance(enemyAI.transform.position, enemyAI.PlayerTarget.position);

        // Condition to STAY in combat: we can see the player OR they are within our lock-on range.
        if (canSeePlayer || distanceToPlayer <= enemyAI.Config.combatLockRange)
        {
            // If we're in attack range, switch to the Attack state and check walk or run.
            if (distanceToPlayer <= enemyAI.Config.walkCombatRange)
            {
                // Player is close, so switch to combat walk.
                enemyAI.Navigator.SetSpeed(enemyAI.Config.walkCombatSpeed);
                enemyAI.AnimController.SetSpeed(enemyAI.Config.walkCombatSpeed);
            }
            else
            {
                // Player is far, so run.
                enemyAI.Navigator.SetSpeed(enemyAI.Config.chaseSpeed);
                enemyAI.AnimController.SetSpeed(enemyAI.Config.chaseSpeed);
            }

            // Check if we are in attack range.
            if (distanceToPlayer <= enemyAI.Config.attackRange)
            {
                enemyAI.TransitionToState(new AttackState());
                return;
            }

            // Always chase the player's CURRENT position while locked on.
            enemyAI.Navigator.MoveTo(enemyAI.PlayerTarget.position);

            // TODO: Add "can't attack" alarm timer logic here.
        }
        else
        {
            // If we've lost them completely (no sight AND outside lock-on range), investigate their last spot.
            enemyAI.LastKnownPlayerPosition = enemyAI.PlayerTarget.position;
            enemyAI.TransitionToState(new AlertState(enemyAI.LastKnownPlayerPosition));
        }
    }

    public override void Exit(EnemyAI enemyAI)
    {
        Debug.Log("Exiting Combat State");
    }
}