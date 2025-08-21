// Create this new script: TariaksuqsRiftEffectSO.cs
using UnityEngine;
using System.Collections;
using System;

[CreateAssetMenu(fileName = "FX_TariaksuqsRift", menuName = "Only The Shadows Know/Skills/Effects/Tariaksuqs Rift Effect")]
public class TariaksuqsRiftEffectSO : SkillEffectSO
{
    [SerializeField] private GameObject riftMarkerPrefab;
    [Tooltip("The minimum distance the player must be from the rift to teleport to it.")]
    [SerializeField] private float maxTeleportRange = 2f;
    [Tooltip("The duration of the cast/teleport animation, during which movement is locked.")]
    [SerializeField] private float castAnimationDuration = 0.5f; // NEW
    [Header("VFX Settings")] // NEW
    [SerializeField] private float vfxLingerDuration = 2f;

    [Header("Specific Audio")]
    [Tooltip("Sound that plays when the rift is cancelled manually or by another skill.")]
    public SoundDefinition cancelSound;

    // Static variables hold the state across all uses of this skill.
    private static Vector3? _riftPosition = null;
    private static GameObject _riftInstance;

    public static bool IsRiftActive => _riftPosition.HasValue;

    static TariaksuqsRiftEffectSO()
    {
        SkillExecutor.OnSkillConfirmed += HandleGlobalSkillConfirmed;
    }

    private static void HandleGlobalSkillConfirmed(SkillSO confirmedSkill)
    {
        // If our rift is active AND the skill that was just used is Spectral Shift...
        if (IsRiftActive && confirmedSkill.skillID == SkillIdentifier.SpectralShift)
        {
            // ...cancel the rift.
            CancelRift();
        }
    }
    public override void Activate(GameObject caster)
    {
        var combatController = caster.GetComponent<PlayerCombat>();
        if (combatController == null) return;

        if (_riftPosition == null) // Placing a new rift
        {
            combatController.RunSkillCoroutine(PlaceRiftRoutine(caster));
        }
        else // Teleporting to the existing rift
        {
            float distanceToRift = Vector3.Distance(caster.transform.position, _riftPosition.Value);
            if (distanceToRift > maxTeleportRange) return;

            combatController.RunSkillCoroutine(TeleportToRiftRoutine(caster));
        }
    }
    private IEnumerator PlaceRiftRoutine(GameObject caster)
    {

        audioProfile.castStartSound.Play(caster.transform);

        var combatController = caster.GetComponent<PlayerCombat>();
        var animController = caster.GetComponent<PlayerAnimationController>();
        var movementController = caster.GetComponent<PlayerMovement>();

        movementController.SetMovementLock(true);
        animController.PlayRiftPlaceAnimation();

        // Spawn the VFX parented to the player and store a reference to it
        GameObject vfxInstance = VFXManager.Instance.PlayRiftPlaceEffect(caster.transform.position, caster.transform);

        // Wait for the animation to play BEFORE placing the rift
        yield return new WaitForSeconds(castAnimationDuration);

        // Place the rift marker (this is not the VFX)
        _riftPosition = caster.transform.position;
        _riftInstance = Instantiate(riftMarkerPrefab, _riftPosition.Value, Quaternion.identity);
        EventManager.Instance.RiftPlaced(_riftPosition.Value, maxTeleportRange);

        // Wait for the animation to finish

        // NOW, tell the VFX to start its cleanup countdown
        if (vfxInstance != null && vfxInstance.TryGetComponent<VFXCleanup>(out var cleanup))
        {
            cleanup.BeginCleanup(vfxLingerDuration);
        }

        movementController.SetMovementLock(false);
    }

    // NEW: Coroutine for the "Teleport to Rift" action
    private IEnumerator TeleportToRiftRoutine(GameObject caster)
    {
        audioProfile.castEndSound.Play(caster.transform);
        var combatController = caster.GetComponent<PlayerCombat>();
        var animController = caster.GetComponent<PlayerAnimationController>();
        var movementController = caster.GetComponent<PlayerMovement>();

        movementController.SetMovementLock(true);
        animController.PlayRiftTeleportAnimation();

        // Spawn the teleport effects (one on player, one on rift)
        GameObject playerVFX = VFXManager.Instance.PlayRiftTeleportEffect(caster.transform.position, caster.transform);
        //GameObject riftVFX = VFXManager.Instance.PlayRiftTeleportEffect(_riftPosition.Value, null); // Rift VFX is not parented

        // Wait for the animation to play BEFORE teleporting
        yield return new WaitForSeconds(castAnimationDuration);
        // Tell both VFX to start their cleanup
        if (playerVFX != null && playerVFX.TryGetComponent<VFXCleanup>(out var pClean)) pClean.BeginCleanup(vfxLingerDuration);
        //if (riftVFX != null && riftVFX.TryGetComponent<VFXCleanup>(out var rClean)) rClean.BeginCleanup(vfxLingerDuration);

        var cc = caster.GetComponent<CharacterController>();
        TeleportManager.Instance.ExecuteTeleport(cc, _riftPosition.Value);
        CancelRift();

        movementController.SetMovementLock(false);
    }

    public static void CancelRift()
    {

        if (GameManager.Instance != null && GameManager.Instance.Player != null)
        {
            // Find the skill on the player to get its unique sound data
            var skill = GameManager.Instance.Player.GetComponent<PlayerSkillController>()?.GetSkill(SkillIdentifier.TariaksuqsRift);
            if (skill != null && skill.skillEffectData is TariaksuqsRiftEffectSO riftEffect)
            {
                riftEffect.cancelSound.Play(GameManager.Instance.Player.transform);
            }
        }

        if (_riftInstance != null)
        {
            Destroy(_riftInstance);
        }
        _riftInstance = null;
        _riftPosition = null;
        EventManager.Instance.RiftCancelledOrUsed();
        Debug.Log("Rift cancelled.");
    }

    // These are not used for this instant-cast skill
    public override IEnumerator StartChannel(GameObject caster, object target = null) { yield break; }
    public override void StopChannel(GameObject caster) { }

}