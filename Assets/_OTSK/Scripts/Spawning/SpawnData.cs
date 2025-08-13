// Create this new script, SpawnData.cs
[System.Serializable]
public class SpawnData
{
    public EnemyConfigSO enemyToSpawn;
    public string spawnPointID;
    public string patrolRouteID;
    public InitialAIState initialState;
}