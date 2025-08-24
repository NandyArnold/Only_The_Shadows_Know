// SaveSlotUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveSlotUI : MonoBehaviour
{
    [SerializeField] private Button loadButton;
    [SerializeField] private TextMeshProUGUI saveNameText; // Or use Text
    private GameObject _parentPanel;

    private string _saveFileName;

    private void Awake()
    {
        if (loadButton == null)
        {
            loadButton = GetComponent<Button>();
        }
        if (saveNameText == null)
        {
            // GetComponentInChildren will find the Text component on this object or any of its children.
            saveNameText = GetComponentInChildren<TextMeshProUGUI>();
        }
        loadButton.onClick.AddListener(OnLoadClicked);
    }

    // The Load Game panel will call this to set up the button.
    public void Initialize(string saveFileName, GameObject parentPanel)
    {
        _saveFileName = saveFileName;
        _parentPanel = parentPanel;
        if (saveNameText != null)
        {
            saveNameText.text = saveFileName;
        }
        else
        {
            Debug.LogError($"SaveSlotUI on '{gameObject.name}' is missing its 'saveNameText' reference!", gameObject);
        }
    }

    private void OnLoadClicked()
    {
        // When clicked, tell the SaveLoadManager to load our specific file.
        if (SaveLoadManager.Instance != null && _parentPanel != null)
        {
            _parentPanel.SetActive(false);
            SaveLoadManager.Instance.LoadGame(_saveFileName);
        }
    }
}