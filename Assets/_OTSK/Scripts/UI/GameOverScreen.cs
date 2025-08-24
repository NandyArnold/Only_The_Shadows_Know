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
        if (AudioManager.Instance != null) AudioManager.Instance.SetGameOver(false);
        Time.timeScale = 1f;
        Debug.Log("Respawn button clicked...");

        // Hide the panel immediately
        gameObject.SetActive(false);

        

        // Tell the GameManager to start the respawn process
        GameManager.Instance.RespawnPlayer();
    }

    private void OnLoadClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SetGameOver(false);
        Time.timeScale = 1f;
        Debug.Log("Load button clicked...");
        gameObject.SetActive(false);
   
        //SaveLoadManager.Instance.LoadGame();
        SaveLoadManager.Instance.LoadGame("autosave");
    }

    private void OnMainMenuClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SetGameOver(false);
        if (GameManager.Instance != null) GameManager.Instance.PrepareForNewScene();
        Debug.Log("Main Menu button clicked...");
        
        SceneLoader.Instance.LoadScene(mainMenuSceneData);
    }

    private void OnExitClicked()
    {
        Debug.Log("Exit button clicked...");
        Application.Quit();
    }
}