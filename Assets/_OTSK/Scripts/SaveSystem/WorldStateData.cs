using System.Collections.Generic;

[System.Serializable]
public class WorldStateData
{
    // We use two lists because Unity can't directly serialize dictionaries.
    public List<string> saveableEntityIDs = new List<string>();
    public List<object> saveableEntityStates = new List<object>();

    public Dictionary<string, object> ToDictionary()
    {
        var dict = new Dictionary<string, object>();
        for (int i = 0; i < saveableEntityIDs.Count; i++)
        {
            dict[saveableEntityIDs[i]] = saveableEntityStates[i];
        }
        return dict;
    }

    public WorldStateData(Dictionary<string, object> state)
    {
        foreach (var item in state)
        {
            saveableEntityIDs.Add(item.Key);
            saveableEntityStates.Add(item.Value);
        }
    }
}
