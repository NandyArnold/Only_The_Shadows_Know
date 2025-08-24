using UnityEngine;
using UnityEngine.UI; // Required for Button component

public class SceneTransitionButton : MonoBehaviour
{
    public Button button; // Assign this in Inspector
    [Header("Scene Settings")]
    [SerializeField] private SceneDataSO targetSceneData;

    [Header("Spawning")]
    [Tooltip("The tag of the spawn point to use in the next scene (e.g., 'InitialSpawn').")]
    [SerializeField] private string targetSpawnPointTag;

   

    private void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }
        else
        {
            Debug.LogError("SceneTransitionButton: Button component not found!");
        }
    }

    private void OnButtonClick()
    {
        if (SceneLoader.Instance != null)
        {
            // --- THIS IS THE FIX ---
            // Before doing anything, tell the GameManager we are starting a NEW game.
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetLoadType(GameLoadType.NewGame);
            }

            SaveLoadManager.Instance.DeleteAllSaves();
            PlayerSpawner.Instance.SetNextSpawnPoint("InitialSpawn");
            SceneLoader.Instance.LoadScene(targetSceneData, targetSpawnPointTag);
        }
        else
        {
            Debug.LogError("SceneTransitionButton: SceneLoader.Instance is null! Is it initialized in BootstrapScene?");
        }
        
    }
}
