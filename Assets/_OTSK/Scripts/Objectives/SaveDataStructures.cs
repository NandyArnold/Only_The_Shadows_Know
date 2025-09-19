using System;
using System.Collections.Generic;

// --- SAVE DATA STRUCTURES ---

[Serializable]
public class ObjectiveInstanceSaveData
{
    public string objectiveID;
    public ObjectiveState state;
    public int goalCurrentAmount;
}

[Serializable]
public class ObjectiveStateData
{
    public string levelChainID;
    public List<ObjectiveInstanceSaveData> objectiveStates;
}

[Serializable]
public class AccomplishmentData
{
    public Dictionary<string, int> killCounts = new Dictionary<string, int>();
    public Dictionary<string, int> destroyCounts = new Dictionary<string, int>();
    public HashSet<string> visitedLocationIDs = new HashSet<string>();
}
