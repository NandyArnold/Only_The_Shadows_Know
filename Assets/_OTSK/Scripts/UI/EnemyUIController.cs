// EnemyUIController.cs
using UnityEngine;
using UnityEngine.UI;

public class EnemyUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider alertSlider;
    [SerializeField] private GameObject deathZoneMark;
    [SerializeField] private Slider castBarSlider;


    [Header("VFX")]
    [Tooltip("Drag the child Alert_VFX GameObject here.")]
    [SerializeField] private GameObject alertVFXObject;


    private Transform _cameraToFace;

    private void Awake()
    {
        // Ensure all bars are hidden by default when the UI is first created.
        if (healthSlider != null) healthSlider.gameObject.SetActive(false);
        if (alertSlider != null) alertSlider.gameObject.SetActive(false);
        if (castBarSlider != null) castBarSlider.gameObject.SetActive(false);
    }


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
    public void UpdateCastBar(float progress)
    {
        // Show the bar if progress is > 0, hide it otherwise
        castBarSlider.gameObject.SetActive(progress > 0);
        castBarSlider.value = progress;
    }

    public void HandleAIStateChanged(EnemyAIState newState)
    {
        // The health bar should be visible in Alert and Combat states.
        bool showHealth = (newState is AlertState || newState is CombatState);
        healthSlider.gameObject.SetActive(showHealth);

        // The alert bar should be hidden when in the Combat state.


        // --- NEW VFX LOGIC ---
        // The VFX should only be active when the AI is in the AlertState.
        bool showAlertUI = !(newState is CombatState);
        alertSlider.gameObject.SetActive(showAlertUI);

        bool shouldShowAlertVFX = (newState is AlertState);
        if (alertVFXObject != null)
        {
            alertVFXObject.SetActive(shouldShowAlertVFX);
        }
    }
    public void SetDeathZoneMarkActive(bool isActive)
    {
        if (deathZoneMark != null)
        {
            deathZoneMark.SetActive(isActive);
        }
    }
}