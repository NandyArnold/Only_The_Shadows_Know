using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelObjectiveChain_", menuName = "Only The Shadows Know/Objectives/Level Objective Chain")]
public class LevelObjectiveChainSO : ScriptableObject
{
    [Tooltip("Unique ID for the level this chain belongs to (e.g., 'Tutorial', 'Mission01').")]
    public string levelID;

    [Tooltip("The ordered list of objectives for this level.")]
    public List<ObjectiveSO> objectives;

    [Tooltip("The tag for the spawn point in the NEXT level where the player should appear.")]
    public string nextLevelSpawnPointTag;
}
