using DG.Tweening; // Make sure you have DOTween imported
using UnityEngine;

[RequireComponent(typeof(Light))]
public class FireLightFlicker : MonoBehaviour
{
    // --- TWEAKABLE SETTINGS ---
    [Header("Intensity Settings")]
    [Tooltip("The minimum light intensity.")]
    [Range(0f, 8f)]
    public float minIntensity = 0.8f;

    [Tooltip("The maximum light intensity.")]
    [Range(0f, 8f)]
    public float maxIntensity = 1.5f;

    [Header("Movement Settings")]
    [Tooltip("How far the light can move from its original position.")]
    public float movementRange = 0.1f;

    [Header("Timing Settings")]
    [Tooltip("The minimum time a flicker can last.")]
    public float minFlickerSpeed = 0.05f;
      

    [Tooltip("The maximum time a flicker can last.")]
    public float maxFlickerSpeed = 0.2f;
  

    // --- PRIVATE VARIABLES ---
    public Light fireLight;
    private Vector3 initialPosition;
    private Sequence flickerSequence;

  
    void Awake()
    {
        // Get the Light component attached to this GameObject
        fireLight = GetComponent<Light>();
        // Store the light's starting position
        initialPosition = transform.localPosition;
    }


    void OnEnable()
    {
        StartFlicker();
    }

    void OnDisable()
    {
        // Make sure to kill tweens when object is disabled/destroyed
        if (flickerSequence != null && flickerSequence.IsActive())
            flickerSequence.Kill();
    }

    /// <summary>
    /// Starts the continuous flickering effect.
    /// </summary>
  

    /// <summary>
    /// Calculates random values and creates the tweens for one flicker cycle.
    /// </summary>
    void StartFlicker()
    {
        // If we already had a sequence, kill it before making a new one
        if (flickerSequence != null && flickerSequence.IsActive())
            flickerSequence.Kill();

        // Create a fresh sequence
        flickerSequence = DOTween.Sequence();

        // Randomize target intensity and duration for this step
        float targetIntensity = Random.Range(minIntensity, maxIntensity);
        float duration = Random.Range(minFlickerSpeed, maxFlickerSpeed);

        // Animate the intensity
        flickerSequence.Append(fireLight.DOIntensity(targetIntensity, duration));

        // When it completes, start again with a new sequence
        flickerSequence.OnComplete(StartFlicker);
    }
}
