using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SaveButton : MonoBehaviour
{
    [Tooltip("The name of the save file to create.")]
    [SerializeField] private string saveFileName = "manual_save_1";

    void Awake()
    {
        // Tell the button's click event to call SaveGame and pass our file name.
        GetComponent<Button>().onClick.AddListener(() => 
        {
            if (SaveLoadManager.Instance != null)
            {
                StartCoroutine(SaveLoadManager.Instance.SaveGame(saveFileName));
            }
        });
    }
}