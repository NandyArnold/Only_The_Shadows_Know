using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "SceneRegistry", menuName = "Only The Shadows Know/Core/Scene Registry")]
public class SceneRegistrySO : ScriptableObject
{
    public List<SceneDataSO> scenes;

    public SceneDataSO GetSceneData(string id)
    {
        return scenes.FirstOrDefault(s => s.sceneID == id);
    }
}
