// SpectralShiftEffectSO.cs

using UnityEngine;

[CreateAssetMenu(fileName = "FX_SpectralShift", menuName = "Only The Shadows Know/Skills/Effects/Spectral Shift Effect")]
public class SpectralShiftEffectSO : SkillEffectSO
{
    public override void Activate(GameObject caster)
    {
        // Try to find the TeleportManager instance.
        if (TeleportManager.Instance == null)
        {
            Debug.LogError("SpectralShiftEffect: TeleportManager not found!");
            return;
        }

        // Try to get the CharacterController from the caster (the player).
        if (!caster.TryGetComponent<CharacterController>(out CharacterController characterController))
        {
            Debug.LogError("SpectralShiftEffect: Caster does not have a CharacterController component!");
            return;
        }

        // 1. Ask the TeleportManager if there is a valid location.
        if (TeleportManager.Instance.RequestLineOfSightTeleport(caster.transform))
        {
            // 2. If yes, tell the TeleportManager to execute the teleport.
            TeleportManager.Instance.ExecuteTeleport(characterController);
        }
        else
        {
            // Optional: Provide feedback if the teleport failed (e.g., play a "fail" sound).
            Debug.Log("Spectral Shift failed: No valid location found.");
        }
    }
}