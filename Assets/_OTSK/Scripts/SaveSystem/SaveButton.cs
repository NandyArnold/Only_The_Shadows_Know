using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(Button))]
public class SaveButton : MonoBehaviour
{
    void Awake() => GetComponent<Button>().onClick.AddListener(() => SaveLoadManager.Instance.SaveGame());
}