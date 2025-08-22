// PlayerController.cs
using System.Collections;
using System.Collections.Generic;
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

    private readonly Dictionary<GameObject, int> _originalLayers = new Dictionary<GameObject, int>();
    private int _playerOriginalLayer;

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

        int spiritWorldLayer = LayerMask.NameToLayer("SpiritWorld");

        if (isActive)
        {
            // Store and change player's layer (and all children)
            _playerOriginalLayer = gameObject.layer;
            SetLayerRecursively(this.gameObject, spiritWorldLayer);

            // Store and change layers of all spirit enemies
            if (EnemyManager.Instance != null)
            {
                _originalLayers.Clear();
                foreach (var enemy in EnemyManager.Instance.GetActiveEnemies())
                {
                    // Use your existing check from the EnemyConfigSO
                    if (enemy.Config.enemyType == EnemyType.Undead /* or other spirit types */)
                    {
                        _originalLayers[enemy.gameObject] = enemy.gameObject.layer;
                        SetLayerRecursively(enemy.gameObject, spiritWorldLayer);
                    }
                }
            }
        }
        else // When turning Endwalker OFF
        {
            // Restore player's layer (and all children)
            SetLayerRecursively(this.gameObject, _playerOriginalLayer);

            // Restore layers of all enemies we changed
            foreach (var entry in _originalLayers)
            {
                if (entry.Key != null) // Check if the enemy still exists
                {
                    SetLayerRecursively(entry.Key, entry.Value);
                }
            }
            _originalLayers.Clear();
        }


        EndwalkerVisuals.Instance.SetEndwalkerActive(isActive);

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
        // Handle the visual effects
        if (VFXManager.Instance != null && EnemyManager.Instance != null)
        {
            if (isActive)
            {
                // Activate aura on the player
                //VFXManager.Instance.PlayEndwalkerAura(this.transform);

                // Activate auras on all "spirit" enemies
                foreach (var enemy in EnemyManager.Instance.GetActiveEnemies())
                {
                    // Use your existing check from the EnemyConfigSO
                    if (enemy.Config.enemyType == EnemyType.Undead /* or other spirit types */)
                    {
                        VFXManager.Instance.PlayEndwalkerAura(enemy.transform);
                    }
                }
            }
            else
            {
                // Deactivate all auras
                VFXManager.Instance.StopAllEndwalkerAuras();
            }
        }
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;

        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            if (child == null) continue;
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    public void SetIsDead(bool isDead) { _isDead = isDead; }
}