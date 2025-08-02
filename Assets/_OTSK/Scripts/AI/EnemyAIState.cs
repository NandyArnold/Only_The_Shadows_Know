// EnemyAIState.cs
using UnityEngine;

public abstract class EnemyAIState
{
    // Called when the state is first entered.
    public abstract void Enter(EnemyAI enemyAI);

    // Called every frame while the state is active.
    public abstract void Execute(EnemyAI enemyAI);

    // Called when the state is exited.
    public abstract void Exit(EnemyAI enemyAI);
}