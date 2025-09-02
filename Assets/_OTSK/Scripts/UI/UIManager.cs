using UnityEngine;
using UnityEngine.UI;
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Panels")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject inGameMenuPanel;

    [Header("Tactical View")]
    [SerializeField] private GameObject tacticalViewPanel; // Assign your TacticalView_Panel
    [SerializeField] private RawImage tacticalViewImage;
    [SerializeField] private TacticalMapController tacticalMapController;

    private PlayerInputHandler _playerInputHandler;

    // You can add other panels here later, like an OptionsMenu panel

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void OnEnable()
    {
        // We need to get a reference to the input handler when the player is ready
        GameManager.Instance.OnPlayerRegistered += RegisterInputHandler;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnPlayerRegistered -= RegisterInputHandler;
        if (_playerInputHandler != null)
        {
            _playerInputHandler.OnCloseMapViewInput -= HandleCloseMapView;
        }
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
                          SceneLoader.Instance.CurrentlyLoadedScene != null &&
                          SceneLoader.Instance.CurrentlyLoadedScene.sceneType == SceneType.Gameplay);
        if (inGameMenuPanel != null)
        {

            inGameMenuPanel.SetActive(showInGameMenu);
            if (showInGameMenu)
            {
                UISoundPlayer.Instance.PlayMenuOpenSound();
                // When the menu becomes active, tell it to update the save button state
                inGameMenuPanel.GetComponent<InGameMenuButtons>().UpdateSaveButtonState();
            }
            else
            {
                UISoundPlayer.Instance.PlayMenuCloseSound();
            }

            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(newState == GameState.GameOver);
            }
        }
    }

    public void ToggleTacticalView()
    {
        if (tacticalViewPanel == null || tacticalMapController == null) return;

        bool shouldBeActive = !tacticalViewPanel.activeSelf;
        tacticalViewPanel.SetActive(shouldBeActive);
        HUDManager.Instance.SetGameplayHUDVisibility(!shouldBeActive);
        if (shouldBeActive)
        {
            // When showing the panel, assign the texture and take control
            if (ScryingSystem.Instance != null && tacticalViewImage != null)
            {
                tacticalViewImage.texture = ScryingSystem.Instance.ScryingRenderTexture;
            }
            tacticalMapController.TakeControl();
            CursorManager.Instance.SetState(CursorState.UI);
        }
        else
        {
            // When hiding the panel, release control
            tacticalMapController.ReleaseControl();
            CursorManager.Instance.SetState(CursorState.Gameplay);
        }
    }

    private void RegisterInputHandler(PlayerController player)
    {
        _playerInputHandler = player.GetComponent<PlayerInputHandler>();
        if (_playerInputHandler != null)
        {
            _playerInputHandler.OnCloseMapViewInput += HandleCloseMapView;
        }
    }

    // This method is called when the 'E' key is pressed on the UI map
    private void HandleCloseMapView()
    {
        // Only toggle if the panel is already active
        if (tacticalViewPanel != null && tacticalViewPanel.activeSelf)
        {
            ToggleTacticalView();
        }
    }

}
