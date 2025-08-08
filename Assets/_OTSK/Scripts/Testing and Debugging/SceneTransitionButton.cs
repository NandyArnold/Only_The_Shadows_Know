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
            PlayerSpawner.Instance.SetNextSpawnPoint("InitialSpawn");

            if (targetSceneData == null)
            {
                Debug.LogError("SceneTransitionButton: Target Scene Data is not assigned!", this.gameObject);
                return;
            }

            // This now calls the correct, updated method on the SceneLoader.
            SceneLoader.Instance.LoadScene(targetSceneData, targetSpawnPointTag);
        }
        else
        {
            Debug.LogError("SceneTransitionButton: SceneLoader.Instance is null! Is it initialized in BootstrapScene?");
        }
        
    }
}
