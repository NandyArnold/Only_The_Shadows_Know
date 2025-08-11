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
        if (!enemyAI.Detector.enabled) return;
        // First, check if we can see a LIVE player.
        // The PlayerTarget property will be null if the player is dead.
        if (enemyAI.PlayerTarget != null && enemyAI.Detector.CanSeePlayer())
        {
            // If we see a live player, escalate to combat.
            enemyAI.TransitionToState(new CombatState());
            return;
        }

        // If we've reached the investigation spot, wait there.
        if (Vector3.Distance(enemyAI.transform.position, _alertLocation) < 1.5f)
        {
            enemyAI.Navigator.Stop();
            enemyAI.AnimController.SetSpeed(0f);

            // Wait for a few seconds before giving up.
            _timer += Time.deltaTime;
            if (_timer >= enemyAI.Config.alertInvestigateTime) // We'll add this variable next
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