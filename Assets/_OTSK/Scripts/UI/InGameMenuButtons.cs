using UnityEngine;
using UnityEngine.UI;
public class InGameMenuButtons : MonoBehaviour
{
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private SceneDataSO mainMenuSceneData;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button loadLastCheckpointButton;
    [SerializeField] private Button loadGameButton;


    private void Awake()
    {
        // Subscribe to the buttons' onClick events
        mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        exitButton.onClick.AddListener(OnExitGameClicked);
        loadLastCheckpointButton.onClick.AddListener(OnLoadLastCheckpointClicked);
        loadGameButton.onClick.AddListener(OnLoadGameClicked);


    }
    public void OnResumeClicked()
    {
        // Tell the GameManager to go back to the Gameplay state
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateGameState(GameState.Gameplay);
        }
    }

    public void OnSaveGameClicked()
    {
        if (SaveLoadManager.Instance != null)
        {
            StartCoroutine(SaveLoadManager.Instance.SaveGame("manual_save_1")); 
        }
    }

    public void OnLoadLastCheckpointClicked()
    {
      
        Debug.Log("Load Last Checkpoint clicked.");
        if (SaveLoadManager.Instance != null)
        {
            // This uses the logic from your old script to load the autosave.
            SaveLoadManager.Instance.LoadGame("autosave");
        }
    }

    public void OnLoadGameClicked()
    {
      
        // Logic to open a "Load Game" screen
        Debug.Log("Load Game clicked.");

        if (SaveLoadManager.Instance != null)
        {
            SaveLoadManager.Instance.LoadGame("manual_save_1"); 
        }
    }
    public void OnOptionsClicked()
    {
        // Logic to open an "Options" screen
        Debug.Log("Options clicked.");
        // Here you would typically open a UI panel for options
    }

    private void OnMainMenuClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SetGameOver(false);
        Debug.Log("Main Menu button clicked...");

      

        SceneLoader.Instance.LoadScene(mainMenuSceneData);
    }

    public void OnExitGameClicked()
    {
        // Logic to exit the game
        Debug.Log("Exit Game clicked.");
        Application.Quit();
    }




    // ... (Add methods for Load, Options, and Main Menu)
}
