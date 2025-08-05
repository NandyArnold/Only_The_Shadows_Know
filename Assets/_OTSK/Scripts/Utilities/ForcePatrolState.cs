using UnityEngine;

[RequireComponent(typeof(EnemyAI))]
public class ForcePatrolState : MonoBehaviour
{
    void Start()
    {
        var enemyAI = GetComponent<EnemyAI>();

        // 1. Force the AI into a new PatrolState instance.
        // We pass 'null' because this dummy doesn't need a patrol route.
        enemyAI.TransitionToState(new PatrolState(null));

        // 2. Disable the EnemyAI component so its Update() method
        //    can't run and it will be stuck in this state forever.
        enemyAI.enabled = false;

        Debug.Log($"'{gameObject.name}' has been forced into PatrolState for testing.");
    }
}
