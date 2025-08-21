// PlayerController.cs
using System.Collections;
using UnityEngine;

/// <summary>
/// The main orchestrator for all player-related components. It holds references
/// to the modular parts and acts as a central point of contact for external systems.
/// </summary>
public class PlayerController : MonoBehaviour
{
    public static event System.Action OnEndwalkerStateEnded;

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
    [Tooltip("The handler for player health and mana.")]
    [SerializeField] private PlayerStats playerStats;

    [Header("Death Settings")] 
    [SerializeField] private float deathAnimationDuration = 2.5f;

    public bool IsDead => _isDead;
    public bool IsInEndwalkerState { get; private set; } = false;
    private PlayerStats _playerStats;
    private bool _isDead = false;

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
        _playerStats = GetComponent<PlayerStats>();
    }

    private void OnEnable()
    {
        if (_playerStats != null) _playerStats.OnDied += HandleDeath;
        if (CombatManager.Instance != null) CombatManager.Instance.OnCombatStart += HandleCombatStart;
    }

    private void OnDisable()
    {
        if (_playerStats != null) _playerStats.OnDied -= HandleDeath;
        if (CombatManager.Instance != null) CombatManager.Instance.OnCombatStart -= HandleCombatStart;
    }

    private void Start()
    {
        // This is the perfect time to announce that the player is fully initialized.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AnnouncePlayerReady();
        }
    }
    public void RespawnAt(Vector3 position, Quaternion rotation)
    {
        // Temporarily disable the character controller to move the player
        var cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        transform.position = position;
        transform.rotation = rotation;

        if (cc != null) cc.enabled = true;

        // Reset the dead state
        _isDead = false;
    }
    private void HandleDeath()
    {
        if (_isDead) return; // Prevent multiple death triggers
        if (IsInEndwalkerState)
        {
            SetEndwalkerState(false);
        }
        StartCoroutine(DeathRoutine());
    }
    private void HandleCombatStart()
    {
        // If we enter combat while in Endwalker, turn it off.
        if (IsInEndwalkerState)
        {
            Debug.Log("Combat started, automatically ending Endwalker state.");
            SetEndwalkerState(false);
        }
    }

    private IEnumerator DeathRoutine()
    {
        if (_isDead) yield break;
        _isDead = true;

        if (AudioManager.Instance != null) AudioManager.Instance.SetGameOver(true);
        //  Tell all enemies to stand down.
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.OnPlayerDeath();
        }
        // Disable inputs
        GetComponent<PlayerInputHandler>().SwitchActionMap("UI");

        // Play death animation
        GetComponent<PlayerAnimationController>().PlayDeathAnimation();

        // Wait for the animation to play
        yield return new WaitForSeconds(deathAnimationDuration);

        // Trigger the Game Over state
        GameManager.Instance.UpdateGameState(GameState.GameOver);
    }




    public void SetEndwalkerState(bool isActive)
    {
        if (IsInEndwalkerState == isActive) return; // Prevent running if already in the state

        IsInEndwalkerState = isActive;
        Debug.Log("Endwalker state set to: " + isActive);

        // Get the skill data from the skill controller to access its audio profile
        var endwalkerSkill = playerSkillController.GetSkill(SkillIdentifier.Endwalker);
        if (endwalkerSkill != null && endwalkerSkill.skillEffectData.audioProfile != null)
        {
            var audioProfile = endwalkerSkill.skillEffectData.audioProfile;

            // Tell the managers to change their state
            SoundEffectManager.Instance.SetEndwalkerState(isActive);


            AudioManager.Instance.MuteNormalMusic(isActive, audioProfile.loopFadeInDuration);

            if (isActive)
            {
                // Play the start sound if one is assigned
                AudioManager.Instance.PlayOverrideAmbience(
                audioProfile.channelLoopSound,
                audioProfile.channelVolume,
                audioProfile.loopFadeInDuration
            );
                // We can still play a start sound if we want
                audioProfile.castStartSound.Play(transform);
            }
            else
            {
                // Stop the loop
                AudioManager.Instance.StopOverrideAmbience(audioProfile.loopFadeOutDuration);

                AudioManager.Instance.RestoreNormalMusic(audioProfile.loopFadeOutDuration);


                // Play the end sound if one is assigned
                audioProfile.castEndSound.Play(transform);
                OnEndwalkerStateEnded?.Invoke();
            }
        }
    }

    // You can add public methods here later if other systems need to interact with the player,
    // for example: public void TeleportTo(Vector3 position) { ... }
    public void SetIsDead(bool isDead) { _isDead = isDead; }
}