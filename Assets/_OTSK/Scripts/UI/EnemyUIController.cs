// EnemyUIController.cs
using UnityEngine;
using UnityEngine.UI;

public class EnemyUIController : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider alertSlider;

    private Transform _cameraToFace;

    

    private void LateUpdate()
    {
        if (_cameraToFace == null)
        {
            if (Camera.main != null)
            {
                _cameraToFace = Camera.main.transform;
            }
            else
            {
                // If there is no main camera in the scene yet, we can't do anything.
                return;
            }
        }
        // Make the canvas billboard towards the camera
        transform.LookAt(transform.position + _cameraToFace.rotation * Vector3.forward, _cameraToFace.rotation * Vector3.up);
    }

    public void UpdateHealth(float current, float max)
    {
        healthSlider.gameObject.SetActive(current < max); // Only show if not at full health
        healthSlider.value = current / max;
    }

    public void UpdateAlert(float current, float max)
    {
        alertSlider.gameObject.SetActive(current > 0); // Only show if there is some alert level
        alertSlider.value = current / max;
    }
}