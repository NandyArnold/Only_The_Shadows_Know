// PlayerController.cs

using UnityEngine;

/// <summary>
/// The main orchestrator for all player-related components. It holds references
/// to the modular parts and acts as a central point of contact for external systems.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Component References")]
    [Tooltip("The handler for player input.")]
    [SerializeField] private PlayerInputHandler playerInputHandler;
    [Tooltip("The handler for player movement and physics.")]
    [SerializeField] private PlayerMovement playerMovement;
    [Tooltip("The handler for player animations.")]
    [SerializeField] private PlayerAnimationController playerAnimationController;
    [Tooltip("The handler for player skills.")]
    [SerializeField] private PlayerSkillController playerSkillController;
    [Tooltip("The handler for player combat actions.")]
    [SerializeField] private PlayerCombat playerCombat;
    [Tooltip("The handler for camera control and rotation.")]
    [SerializeField] private CameraController cameraController;
    [Tooltip("The handler for player health, mana, and noise.")]
    [SerializeField] private PlayerHealthManaNoise playerHealthManaNoise;

    // We can add references to PlayerCombat, PlayerHealthManaNoise, etc. here later.

    void Awake()
    {
        // On Awake, this controller registers itself with the GameManager.
        // This makes the entire Player object findable by any other system.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterPlayer(this);
        }
        else
        {
            Debug.LogError("PlayerController could not find GameManager.Instance to register with!", this);
        }
    }

    // You can add public methods here later if other systems need to interact with the player,
    // for example: public void TeleportTo(Vector3 position) { ... }
}