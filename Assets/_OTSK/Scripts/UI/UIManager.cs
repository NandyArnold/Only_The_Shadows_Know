using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Panels")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject inGameMenuPanel;
    // You can add other panels here later, like an OptionsMenu panel

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        }
        // Ensure all panels are hidden at the start
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (inGameMenuPanel != null) inGameMenuPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }
    }

    private void HandleGameStateChanged(GameState newState)
    {

        bool showInGameMenu = (newState == GameState.Menu &&
                           SceneLoader.Instance.CurrentlyLoadedScene.sceneType == SceneType.Gameplay);
        inGameMenuPanel.SetActive(showInGameMenu);



        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(newState == GameState.GameOver);
        }

        if (inGameMenuPanel != null)
        {
            // The in-game menu is our "pause" screen
            inGameMenuPanel.SetActive(newState == GameState.Menu);
        }

    }
}