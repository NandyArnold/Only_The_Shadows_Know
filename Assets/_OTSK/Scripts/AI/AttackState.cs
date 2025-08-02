// AttackState.cs
using UnityEngine;

public class AttackState : EnemyAIState
{
   
    private float _timer;

    public override void Enter(EnemyAI enemyAI)
    {
        Debug.Log("Entering Attack State");
        enemyAI.Navigator.Stop(); // Stop moving to attack
        enemyAI.AnimController.SetSpeed(0f);

        // Make the enemy face the player
        if (enemyAI.PlayerTarget != null)
        {
            enemyAI.transform.LookAt(enemyAI.PlayerTarget);
        }

        enemyAI.GetComponent<EnemyCombatHandler>().PerformAttack();
        _timer = 0f;
    }

    public override void Execute(EnemyAI enemyAI)
    {
        _timer += Time.deltaTime;

        // After the cooldown, go back to the Combat state to re-evaluate.
        if (_timer > enemyAI.Config.attackCooldown)
        {
            enemyAI.TransitionToState(new CombatState());
        }
    }

    public override void Exit(EnemyAI enemyAI)
    {
        Debug.Log("Exiting Attack State");
    }
}