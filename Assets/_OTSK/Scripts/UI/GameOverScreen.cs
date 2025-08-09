using DG.Tweening.Core.Easing;
using UnityEngine;
using UnityEngine.UI;

public class GameOverScreen : MonoBehaviour
{
    [SerializeField] private Button respawnButton;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button mainMenuButton;
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
        // We will add the logic for this when we build the CheckpointManager
    }

    private void OnLoadClicked()
    {
        Debug.Log("Load button clicked...");
        SaveLoadManager.Instance.LoadGame();
    }

    private void OnMainMenuClicked()
    {
        Debug.Log("Main Menu button clicked...");
        // You'll need a SceneDataSO asset for your MainMenu scene
         //SceneLoader.Instance.LoadScene(mainMenuSceneData); 
    }

    private void OnExitClicked()
    {
        Debug.Log("Exit button clicked...");
        Application.Quit();
    }
}