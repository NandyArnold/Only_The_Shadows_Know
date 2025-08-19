using System.Collections.Generic;

[System.Serializable]
public class WorldStateData
{
    // We use two lists because Unity can't directly serialize dictionaries.
    // We now have a dedicated, strongly-typed list for each type of saveable object.
    public Dictionary<string, Enemy.EnemySaveData> enemySaveData = new Dictionary<string, Enemy.EnemySaveData>();
    public Dictionary<string, ChargeManager.ChargeSaveData> chargeManagerSaveData = new Dictionary<string, ChargeManager.ChargeSaveData>();
    public Dictionary<string, Checkpoint.CheckpointSaveData> checkpointSaveData = new Dictionary<string, Checkpoint.CheckpointSaveData>();


}
