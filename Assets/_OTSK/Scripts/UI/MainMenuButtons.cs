using UnityEngine;
using UnityEngine.UI;

public class MainMenuButtons : MonoBehaviour
{
    [Header("Button References")]
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button exitGameButton;
            

    private string _lastSaveName;


    private void Awake()
    {
        loadGameButton.onClick.AddListener(OnLoadGameClicked);
        optionsButton.onClick.AddListener(OnOptionsClicked);
        exitGameButton.onClick.AddListener(OnExitGameClicked);

    }

    private void Start()
    {
        if (loadGameButton != null && SaveLoadManager.Instance != null)
        {
            // Get the name of the last saved game from our new method
            _lastSaveName = SaveLoadManager.Instance.GetLastSaveName();

            // Enable the button only if a last save name exists AND the file for it exists.
            bool canLoad = !string.IsNullOrEmpty(_lastSaveName) &&
                           SaveLoadManager.Instance.DoesSaveExist(_lastSaveName);

            loadGameButton.interactable = canLoad;
        }
    }


    public void OnLoadGameClicked()
    {
        Debug.Log("Load Game clicked.");
        if (SaveLoadManager.Instance != null && !string.IsNullOrEmpty(_lastSaveName))
        {
            Debug.Log($"<color=cyan>Loading last saved game: {_lastSaveName}</color>");
            // Load the last saved game, whatever its name was.
            SaveLoadManager.Instance.LoadGame(_lastSaveName);
        }
    }


    public void OnOptionsClicked()
    {
        // We will add logic here to open an "Options" screen later
        Debug.Log("Options clicked.");
    }

    public void OnExitGameClicked()
    {
        Application.Quit();
    }
}