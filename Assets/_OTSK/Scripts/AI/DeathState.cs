// DeathState.cs
using UnityEngine;

public class DeathState : EnemyAIState
{
    public override void Enter(EnemyAI enemyAI)
    {
        // This state does nothing. Its purpose is to be the final,
        // unambiguous "dead" state for the AI state machine.
        // The actual death logic (disabling components, playing animations)
        // is handled in Enemy.cs's HandleDeath method.
    }

    public override void Execute(EnemyAI enemyAI)
    {
        // Does nothing.
    }

    public override void Exit(EnemyAI enemyAI)
    {
        // Does nothing.
    }
}