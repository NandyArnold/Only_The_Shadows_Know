using UnityEngine;

public class AutosaveManager : MonoBehaviour
{
    public static AutosaveManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    public void PerformAutosave(SceneDataSO sceneData)
    {
        
        if (sceneData.sceneType == SceneType.Gameplay)
        {
            if (SaveLoadManager.Instance != null)
            {
                Debug.Log($"<color=cyan>AUTOSAVING...</color> Scene: {sceneData.SceneName}");
                SaveLoadManager.Instance.SaveGame("autosave");
            }
        }
    }
}