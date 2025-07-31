using UnityEngine;
using UnityEngine.UI; // Required for Button component

public class SceneTransitionButton : MonoBehaviour
{
    public Button button; // Assign this in Inspector
    public string targetSceneName; // Set this in Inspector

    private void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }
        else
        {
            Debug.LogError("SceneTransitionButton: Button component not found!");
        }
    }

    private void OnButtonClick()
    {
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadSceneAsync(targetSceneName);
        }
        else
        {
            Debug.LogError("SceneTransitionButton: SceneLoader.Instance is null! Is it initialized in BootstrapScene?");
        }
    }
}
