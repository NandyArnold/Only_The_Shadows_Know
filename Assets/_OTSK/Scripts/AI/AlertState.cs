// AlertState.cs
using UnityEngine;

public class AlertState : EnemyAIState
{
    private readonly Vector3 _alertLocation;
    private float _timer;
    private float _investigateDuration = 5f;

    public AlertState(Vector3 alertLocation)
    {
        _alertLocation = alertLocation;
    }

    public override void Enter(EnemyAI enemyAI)
    {
        Debug.Log("Entering Alert State");
        enemyAI.Navigator.SetSpeed(enemyAI.Config.alertSpeed);
        enemyAI.AnimController.SetSpeed(enemyAI.Config.alertSpeed);

        enemyAI.Navigator.MoveTo(_alertLocation);
        _timer = 0f;
    }

    public override void Execute(EnemyAI enemyAI)
    {
        Debug.Log("Executing Alert State");
        // If we get a visual on the player and are close enough, escalate to combat.
        if (enemyAI.Detector.CanSeePlayer() &&
            Vector3.Distance(enemyAI.transform.position, enemyAI.PlayerTarget.position) <= enemyAI.Config.combatEntryRange)
        {
            enemyAI.TransitionToState(new CombatState());
            return;
        }

        // Check if we've arrived at the alert spot.
        if (Vector3.Distance(enemyAI.transform.position, _alertLocation) < 1.5f)
        {
            enemyAI.Navigator.Stop();
            enemyAI.AnimController.SetSpeed(0f);
            _timer += Time.deltaTime;

            // If we've waited long enough and found nothing, go back to patrolling.
            if (_timer > _investigateDuration)
            {
                enemyAI.TransitionToState(new PatrolState(enemyAI.PatrolRoute));
            }
        }
    }

    public override void Exit(EnemyAI enemyAI)
    {
        Debug.Log("Exiting Alert State");
    }
}