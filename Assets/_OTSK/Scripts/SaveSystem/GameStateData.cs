[System.Serializable]
public class GameStateData
{
    public PlayerStateData playerData;
    public ObjectiveStateData objectiveData;
    public WorldStateData worldData;
    public string sceneID;
    public CheckpointManagerSaveData checkpointData;
    public AccomplishmentData accomplishmentData;

    public GameStateData()
    {
        playerData = new PlayerStateData();
        objectiveData = new ObjectiveStateData();
        worldData = new WorldStateData();
    }
}