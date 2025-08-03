// TeleportManager.cs

using System;
using UnityEngine;

/// <summary>
/// Manages and executes all player teleportation, including location validation.
/// </summary>
public class TeleportManager : MonoBehaviour
{
    public static TeleportManager Instance { get; private set; }

    [Header("Teleport Settings")]
    [SerializeField] private float maxTeleportDistance = 15f;
    [SerializeField] private LayerMask validTeleportLayerMask; // Layers we can teleport onto (e.g., "Ground")

    [Header("Validation Settings")]
    [Tooltip("The maximum angle of a slope the player can teleport onto.")]
    [SerializeField] private float maxGroundAngle = 45f;
    [Tooltip("How far above the target point to cast down from to find the ground.")]
    [SerializeField] private float groundCheckOffset = 2f;

    [Header("Player Dimensions")] 
    [SerializeField] private float playerHeight = 2f;
    [SerializeField] private float playerRadius = 0.5f;

    [Header("Feedback")]
    [SerializeField] private GameObject teleportVFXPrefab;
    // We'll add SFX later when the AudioManager is more developed.

    private Vector3 _validatedTeleportPosition;

    // Events for other systems to listen to
    public event Action OnTeleportCompleted;
    public event Action<string> OnTeleportFailed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Checks for a valid teleport location in a given direction from the player.
    /// </summary>
    /// <returns>True if a valid location was found.</returns>
    /// // NEW: A more flexible validation method
    public bool IsValidTeleportLocation(Vector3 position, out Vector3 validGroundPosition)
    {
        validGroundPosition = position;

        // Check if the area is physically clear for the player to stand in.
        if (Physics.CheckCapsule(position + Vector3.up * playerRadius, position + Vector3.up * (playerHeight - playerRadius), playerRadius, ~0, QueryTriggerInteraction.Ignore))
        {
            return false; // Something is blocking the capsule.
        }

        // Check for flat ground underneath.
        if (Physics.Raycast(position + Vector3.up * 0.1f, Vector3.down, out RaycastHit hit, 1f, validTeleportLayerMask))
        {
            if (IsLocationFlatEnough(hit.normal))
            {
                validGroundPosition = hit.point;
                return true;
            }
        }

        return false;
    }
    public bool RequestLineOfSightTeleport(Transform playerTransform)
    {
        // Raycast forward to find a potential obstacle or distant ground point.
        if (Physics.Raycast(playerTransform.position, playerTransform.forward, out RaycastHit hit, maxTeleportDistance, validTeleportLayerMask))
        {
            // Now, raycast down from above the hit point to find the actual ground surface.
            // This handles teleporting onto ledges or down slopes.
            if (Physics.Raycast(hit.point + Vector3.up * groundCheckOffset, Vector3.down, out RaycastHit groundHit, groundCheckOffset * 2, validTeleportLayerMask))
            {
                // Check if the ground is flat enough.
                if (IsLocationFlatEnough(groundHit.normal))
                {
                    _validatedTeleportPosition = groundHit.point;
                    return true;
                }
                else
                {
                    OnTeleportFailed?.Invoke("Target location is too steep.");
                    return false;
                }
            }
        }

        OnTeleportFailed?.Invoke("No valid location in range.");
        return false;
    }

    /// <summary>
    /// Executes the teleport, moving the player to the pre-validated position.
    /// </summary>
    public void ExecuteTeleport(CharacterController playerCharacterController, Vector3 position)
    {
        // Disable the controller to teleport, then re-enable it.
        playerCharacterController.enabled = false;
        playerCharacterController.transform.position = position;
        playerCharacterController.enabled = true;

        // Play visual effects
        if (teleportVFXPrefab != null)
        {
            Instantiate(teleportVFXPrefab, _validatedTeleportPosition, Quaternion.identity);
        }

        OnTeleportCompleted?.Invoke();
        Debug.Log($"Teleported to {_validatedTeleportPosition}");
    }

    private bool IsLocationFlatEnough(Vector3 groundNormal)
    {
        float angle = Vector3.Angle(Vector3.up, groundNormal);
        return angle <= maxGroundAngle;
    }
}