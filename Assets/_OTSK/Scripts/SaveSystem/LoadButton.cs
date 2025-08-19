using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class LoadButton : MonoBehaviour
{
    [Tooltip("The name of the save file to load.")]
    [SerializeField] private string saveFileName = "autosave";

    void Awake()
    {
        // Tell the button's click event to call LoadGame and pass our file name.
        GetComponent<Button>().onClick.AddListener(() =>
        {
            if (SaveLoadManager.Instance != null)
            {
                SaveLoadManager.Instance.LoadGame(saveFileName);
            }
        });
    }
}