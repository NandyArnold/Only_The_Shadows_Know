using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class InGameMenuButtons : MonoBehaviour
{
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private SceneDataSO mainMenuSceneData;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button loadLastCheckpointButton;
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Button saveGameButton;

    [Header("Save Game UI")]
    [SerializeField] private GameObject saveGamePanel; 
    [SerializeField] private TMP_InputField saveNameInputField; 
    [SerializeField] private Button confirmSaveButton;
    [SerializeField] private Button cancelSaveButton;


    [Header("Load Game UI")]
    [SerializeField] private GameObject loadGamePanel;
    [SerializeField] private Button loadAutosaveButton;
    [SerializeField] private Transform manualSaveContentArea; // The "Content" object of your Scroll View
    [SerializeField] private GameObject saveSlotPrefab;

    private PlayerInputHandler _playerInputHandler;

    private void Awake()
    {
        // Subscribe to the buttons' onClick events
        mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        exitButton.onClick.AddListener(OnExitGameClicked);
        loadLastCheckpointButton.onClick.AddListener(OnLoadLastCheckpointClicked);
        loadGameButton.onClick.AddListener(OnLoadGameClicked);
        confirmSaveButton.onClick.AddListener(OnConfirmSaveClicked);
        cancelSaveButton.onClick.AddListener(OnCancelSaveClicked);
        loadAutosaveButton.onClick.AddListener(OnLoadAutosaveClicked);

        saveNameInputField.onSelect.AddListener(OnInputFieldSelected);
        saveNameInputField.onDeselect.AddListener(OnInputFieldDeselected);
    }
    public void OnResumeClicked()
    {
        // Tell the GameManager to go back to the Gameplay state
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateGameState(GameState.Gameplay);
        }
    }

    private void Start()
    {
        // --- NEW: Get a reference to the PlayerInputHandler ---
        if (GameManager.Instance != null && GameManager.Instance.Player != null)
        {
            _playerInputHandler = GameManager.Instance.Player.GetComponent<PlayerInputHandler>();
        }
    }

    public void OnSaveGameClicked()
    {
        saveGamePanel.SetActive(true);
     
        saveNameInputField.text = GetDefaultSaveName();
     
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
        loadGamePanel.SetActive(true);
        PopulateLoadMenu();
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




    private string GetDefaultSaveName()
    {
        int i = 1;
        // Loop indefinitely until we find a name that doesn't exist.
        while (true)
        {
            string defaultName = $"manual_save_{i}";
            if (!SaveLoadManager.Instance.DoesSaveExist(defaultName))
            {
                return defaultName; // We found an available slot.
            }
            i++; // Try the next number.
        }
    }
    private void OnConfirmSaveClicked()
    {
        string saveFileName = saveNameInputField.text;

        // Use the exact same logic as before to get the name.
        if (string.IsNullOrWhiteSpace(saveFileName))
        {
            saveFileName = GetDefaultSaveName();
        }

        if (SaveLoadManager.Instance != null)
        {
            StartCoroutine(SaveLoadManager.Instance.SaveGame(saveFileName));
        }

        // Hide the panel after confirming.
        saveGamePanel.SetActive(false);
        OnInputFieldDeselected("");
    }

    // This method is called by the new "Cancel" button.
    private void OnCancelSaveClicked()
    {
        saveGamePanel.SetActive(false);
        OnInputFieldDeselected("");
    }

    private void OnLoadAutosaveClicked()
    {
        if (SaveLoadManager.Instance != null)
        {
            loadGamePanel.SetActive(false);
            SaveLoadManager.Instance.LoadGame("autosave");
        }
    }

    private void PopulateLoadMenu()
    {
        List<GameObject> slotsToDestroy = new List<GameObject>();
        foreach (Transform child in manualSaveContentArea)
        {
            slotsToDestroy.Add(child.gameObject);
        }

        // Step 2: Now, loop through the temporary list and destroy the objects.
        // This is safer because we are not modifying the list we are iterating over.
        foreach (GameObject slot in slotsToDestroy)
        {
            Destroy(slot);
        }
        // --- END OF FIX ---

        // Get all manual save file names.
        List<string> manualSaves = SaveLoadManager.Instance.GetAllManualSaveNames();

        Debug.Log($"<color=cyan>[PopulateLoadMenu]</color> Found {manualSaves.Count} manual save(s) to create buttons for.");

        // Create a new button for each manual save.
        foreach (string saveName in manualSaves)
        {
            Debug.Log($"--> Creating button for: {saveName}");
            GameObject slotInstance = Instantiate(saveSlotPrefab, manualSaveContentArea);
            slotInstance.GetComponent<SaveSlotUI>().Initialize(saveName, loadGamePanel);
        }

        // Disable the autosave button if no autosave exists.
        loadAutosaveButton.interactable = SaveLoadManager.Instance.DoesSaveExist("autosave");
    }

    public void UpdateSaveButtonState()
    {
        bool canSave = !CombatManager.Instance.IsPlayerInCombat && !EnemyManager.Instance.IsAnyEnemyAlerted();
        if (saveGameButton != null)
        {
            saveGameButton.interactable = canSave;
        }
    }

    
    private void OnInputFieldSelected(string text)
    {
        if (_playerInputHandler != null)
        {
            _playerInputHandler.SwitchActionMap("Typing");
        }
    }

    private void OnInputFieldDeselected(string text)
    {
        if (_playerInputHandler != null)
        {
            // When we stop typing, we go back to the standard UI map
            _playerInputHandler.SwitchActionMap("UI");
        }
    }
}
