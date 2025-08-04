using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SceneRestartButton : MonoBehaviour
{
    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        // Simply tells the SceneLoader to restart whatever scene is currently loaded.
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.RestartCurrentScene();
        }
    }
}