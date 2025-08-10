using DG.Tweening.Core.Easing;
using UnityEngine;
using UnityEngine.UI;

public class GameOverScreen : MonoBehaviour
{
    [SerializeField] private Button respawnButton;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private SceneDataSO mainMenuSceneData;
    [SerializeField] private Button exitButton;

    private void Awake()
    {
        // Subscribe to the buttons' onClick events
        respawnButton.onClick.AddListener(OnRespawnClicked);
        loadButton.onClick.AddListener(OnLoadClicked);
        mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        exitButton.onClick.AddListener(OnExitClicked);

    

        // Start hidden
        gameObject.SetActive(false);
    }

  



    private void OnRespawnClicked()
    {
        Debug.Log("Respawn button clicked...");
        gameObject.SetActive(false);
        // We will add the logic for this when we build the CheckpointManager
    }

    private void OnLoadClicked()
    {
        Time.timeScale = 1f;
        Debug.Log("Load button clicked...");
        gameObject.SetActive(false);
        SaveLoadManager.Instance.LoadGame();
    }

    private void OnMainMenuClicked()
    {
        Debug.Log("Main Menu button clicked...");
        
        SceneLoader.Instance.LoadScene(mainMenuSceneData);
    }

    private void OnExitClicked()
    {
        Debug.Log("Exit button clicked...");
        Application.Quit();
    }
}