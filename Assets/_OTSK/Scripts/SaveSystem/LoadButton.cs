using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(Button))]
public class LoadButton : MonoBehaviour
{
    void Awake() => GetComponent<Button>().onClick.AddListener(() => SaveLoadManager.Instance.LoadGame());
}