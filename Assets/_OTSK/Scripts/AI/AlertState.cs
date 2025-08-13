// AlertState.cs
using UnityEngine;
using UnityEngine.AI;

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
        Debug.Log($"<color=yellow>[AlertState] ENTER:</color> Investigating location {_alertLocation}", enemyAI.gameObject);
        _timer = 0f;

        

        // 1. Configure the Navigator's properties first.
        enemyAI.Navigator.SetStoppingDistance(1.5f);
        enemyAI.Navigator.SetSpeed(enemyAI.Config.alertSpeed);
        enemyAI.AnimController.SetSpeed(enemyAI.Config.alertSpeed);

        // 2. Find a valid and reachable point on the NavMesh.
        if (NavMesh.SamplePosition(_alertLocation, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            NavMeshPath path = new NavMeshPath();
            if (enemyAI.Navigator.CalculatePath(hit.position, path) && path.status == NavMeshPathStatus.PathComplete)
            {
                // 3. If a valid path is found, tell the agent to RESUME and GO.
                enemyAI.Navigator.Resume();
                enemyAI.Navigator.MoveTo(hit.position);
                return; // Exit successfully
            }
        }

        // If either of the checks above failed, we can't reach the location.
        Debug.LogWarning("AlertState could not find a valid/reachable NavMesh point. Investigating on the spot.", enemyAI.gameObject);
        enemyAI.Navigator.Stop();
        enemyAI.AnimController.SetSpeed(0);

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
        if (enemyAI.Navigator.HasReachedDestination)
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