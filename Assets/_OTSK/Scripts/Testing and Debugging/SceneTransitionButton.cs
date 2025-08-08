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

    [Header("Loading Screen")]
    [Tooltip("Check this to show the loading screen during this transition.")]
    [SerializeField] private bool showLoadingScreen = true;

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

            SceneLoader.Instance.LoadScene(targetSceneData, targetSpawnPointTag, showLoadingScreen);
        }
        else
        {
            Debug.LogError("SceneTransitionButton: SceneLoader.Instance is null! Is it initialized in BootstrapScene?");
        }
        
    }
}
