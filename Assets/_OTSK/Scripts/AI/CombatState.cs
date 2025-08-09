// CombatState.cs - CORRECTED CHASE LOGIC

using UnityEngine;

public class CombatState : EnemyAIState
{
    private enum CombatSubState { Chasing, Attacking }
    private CombatSubState _subState;

    private float _attackCooldownTimer;
    private float _attackAnimationTimer;
    public override void Enter(EnemyAI enemyAI)
    {
        CombatManager.Instance.RegisterEnemyInCombat(enemyAI.GetComponent<Enemy>());
        enemyAI.Navigator.Resume();
        enemyAI.AnimController.SetIsInCombat(true);
        _attackCooldownTimer = 0f; // Reset attack timer so they can attack immediately

        // We always start by chasing the player.
        _subState = CombatSubState.Chasing;
    }

    public override void Execute(EnemyAI enemyAI)
    {
        if (enemyAI.PlayerTarget == null)
        {
            enemyAI.TransitionToState(new PatrolState(enemyAI.PatrolRoute));
            return;
        }

        // Always face the player while in combat
        Vector3 playerPositionOnPlane = new Vector3(enemyAI.PlayerTarget.position.x, enemyAI.transform.position.y, enemyAI.PlayerTarget.position.z);
        enemyAI.transform.LookAt(playerPositionOnPlane);

        _attackCooldownTimer -= Time.deltaTime;
        float distanceToPlayer = Vector3.Distance(enemyAI.transform.position, enemyAI.PlayerTarget.position);

        switch (_subState)
        {
            case CombatSubState.Chasing:
                // --- CHASING LOGIC ---
                enemyAI.Navigator.Resume();

                if (distanceToPlayer <= enemyAI.Config.walkCombatRange)
                {
                    enemyAI.Navigator.SetSpeed(enemyAI.Config.walkCombatSpeed);
                    enemyAI.AnimController.SetSpeed(enemyAI.Config.walkCombatSpeed);
                }
                else
                {
                    enemyAI.Navigator.SetSpeed(enemyAI.Config.chaseSpeed);
                    enemyAI.AnimController.SetSpeed(enemyAI.Config.chaseSpeed);
                }
                enemyAI.Navigator.MoveTo(enemyAI.PlayerTarget.position);

                // If we get in attack range AND our main cooldown is ready, switch to Attacking.
                if (distanceToPlayer <= enemyAI.Config.attackRange && _attackCooldownTimer <= 0f)
                {
                    _subState = CombatSubState.Attacking;

                    // Stop moving and perform the attack ONE TIME.
                    enemyAI.Navigator.Stop();
                    enemyAI.AnimController.SetSpeed(0);
                    enemyAI.CombatHandler.PerformAttack();

                    // Set the animation timer.
                    _attackAnimationTimer = enemyAI.Config.attackAnimationDuration;
                }
                break;

            case CombatSubState.Attacking:
                // --- ATTACKING LOGIC ---
                // We are now "stuck" in this state, frozen in place,
                // while the animation plays. We just count down the timer.
                _attackAnimationTimer -= Time.deltaTime;
                if (_attackAnimationTimer <= 0f)
                {
                    // Animation is finished. Reset the main attack cooldown and go back to chasing.
                    _attackCooldownTimer = enemyAI.Config.attackCooldown;
                    _subState = CombatSubState.Chasing;
                }
                break;
        }

        // This is the check to exit the ENTIRE combat state.
        if (distanceToPlayer > enemyAI.Config.combatLockRange)
        {
            enemyAI.LastKnownPlayerPosition = enemyAI.PlayerTarget.position;
            enemyAI.TransitionToState(new AlertState(enemyAI.LastKnownPlayerPosition));
        }
    }

    public override void Exit(EnemyAI enemyAI)
    {
        Debug.Log("Exiting Combat State");
        enemyAI.AnimController.SetIsInCombat(false);
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.UnregisterEnemyFromCombat(enemyAI.GetComponent<Enemy>());
        }
    }
}