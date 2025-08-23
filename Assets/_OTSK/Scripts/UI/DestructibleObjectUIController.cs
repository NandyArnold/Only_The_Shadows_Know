// Create this new script: DestructibleUIController.cs
using UnityEngine;
using UnityEngine.UI;

public class DestructibleUIController : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    private Transform _cameraToFace;
    private GameObject _revealIconInstance;
    private void Awake()
    {
        // Ensure the bar is hidden when the object is created.
        if (healthSlider != null)
        {
            healthSlider.gameObject.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        // This makes the UI always face the camera
        if (_cameraToFace == null)
        {
            if (Camera.main != null) _cameraToFace = Camera.main.transform;
            else return;
        }
        transform.LookAt(transform.position + _cameraToFace.rotation * Vector3.forward,
                         _cameraToFace.rotation * Vector3.up);
    }

    // This is the only method we need to control the health bar
    public void UpdateHealth(float current, float max)
    {
        if (healthSlider == null) return;

        // Show the health bar only if health is not full, and hide it otherwise.
        healthSlider.gameObject.SetActive(current < max && current > 0);
        healthSlider.value = current / max;
    }

    public void InitializeRevealIcon(GameObject iconPrefab)
    {
        if (iconPrefab != null)
        {
            // The code looks for a child object named "RevealIcon_Anchor"
            Transform anchor = transform.Find("RevealIcon_Anchor");
            if (anchor != null)
            {
                _revealIconInstance = Instantiate(iconPrefab, anchor);
                _revealIconInstance.SetActive(false); // Starts hidden
            }
            else
            {
                Debug.LogWarning("Could not find 'RevealIcon_Anchor' child object on the UI prefab.", this.gameObject);
            }
        }
    }

    // ADD THIS METHOD: Allows other scripts to show or hide the icon
    public void SetRevealIconActive(bool isActive)
    {
        if (_revealIconInstance != null)
        {
            _revealIconInstance.SetActive(isActive);
        }
    }
}