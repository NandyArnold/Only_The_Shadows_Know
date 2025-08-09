// CombatState.cs - CORRECTED CHASE LOGIC

using UnityEngine;

public class CombatState : EnemyAIState
{
    private float _attackCooldownTimer;
    public override void Enter(EnemyAI enemyAI)
    {
        Debug.Log("Entering Combat State");
        //Pass a reference to the enemy.
        CombatManager.Instance.RegisterEnemyInCombat(enemyAI.GetComponent<Enemy>());
        enemyAI.Navigator.Resume();

        enemyAI.Navigator.SetSpeed(enemyAI.Config.chaseSpeed);
        enemyAI.AnimController.SetSpeed(enemyAI.Config.chaseSpeed);
        
    }

    public override void Execute(EnemyAI enemyAI)
    {
        if (enemyAI.PlayerTarget == null)
        {
            enemyAI.TransitionToState(new PatrolState(enemyAI.PatrolRoute));
            return;
        }

        // Always face the player while in combat
        enemyAI.transform.LookAt(enemyAI.PlayerTarget);

        _attackCooldownTimer -= Time.deltaTime;
        float distanceToPlayer = Vector3.Distance(enemyAI.transform.position, enemyAI.PlayerTarget.position);

        // Check if we should stop chasing and start attacking
        if (distanceToPlayer <= enemyAI.Config.attackRange)
        {
            enemyAI.Navigator.Stop();
            enemyAI.AnimController.SetSpeed(0);

            // If cooldown is ready, attack
            if (_attackCooldownTimer <= 0f)
            {
                enemyAI.CombatHandler.PerformAttack();
                _attackCooldownTimer = enemyAI.Config.attackCooldown;
            }
        }
        else // If we are out of attack range, chase the player
        {
            enemyAI.Navigator.Resume();
            enemyAI.Navigator.MoveTo(enemyAI.PlayerTarget.position);
            enemyAI.Navigator.SetSpeed(enemyAI.Config.chaseSpeed);
            enemyAI.AnimController.SetSpeed(enemyAI.Config.chaseSpeed);
        }

        // If we lose the player completely, go back to alert
        if (distanceToPlayer > enemyAI.Config.combatLockRange)
        {
            enemyAI.LastKnownPlayerPosition = enemyAI.PlayerTarget.position;
            enemyAI.TransitionToState(new AlertState(enemyAI.LastKnownPlayerPosition));
        }
    }

    public override void Exit(EnemyAI enemyAI)
    {
        Debug.Log("Exiting Combat State");
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.UnregisterEnemyFromCombat(enemyAI.GetComponent<Enemy>());
        }
    }
}