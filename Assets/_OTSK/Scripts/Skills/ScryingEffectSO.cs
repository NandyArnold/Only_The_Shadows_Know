// ScryingEffectSO.cs - FINAL VERSION
using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "FX_Scrying", menuName = "Only The Shadows Know/Skills/Effects/Scrying Effect")]
public class ScryingEffectSO : SkillEffectSO
{
    [Tooltip("The duration of the cast animation, during which movement is locked.")]
    [SerializeField] private float castAnimationDuration = 1.0f;
    [SerializeField] private float vfxLingerDuration = 2.0f;

    // This is called by the "toggle-off" logic in PlayerSkillController
    public override void Activate(GameObject caster)
    {
        // If the skill is already active, this call will deactivate it.
        if (ScryingSystem.Instance != null && ScryingSystem.Instance.IsScryingActive)
        {
            ScryingSystem.Instance.DeactivateScrying();
        }
    }

    // This is called by the "toggle-on" logic for Targeted skills
    public override IEnumerator StartChannel(GameObject caster, object target = null)
    {
        var movementController = caster.GetComponent<PlayerMovement>();
        var animController = caster.GetComponent<PlayerAnimationController>();

        // --- CASTING SEQUENCE ---
        movementController.SetMovementLock(true);
        animController.PlayScryingCastAnimation();
        GameObject vfxInstance = VFXManager.Instance.PlayScryingCastEffect(caster.transform.position, caster.transform);

        yield return new WaitForSeconds(castAnimationDuration);

        if (vfxInstance != null && vfxInstance.TryGetComponent<VFXCleanup>(out var cleanup))
        {
            cleanup.BeginCleanup(vfxLingerDuration);
        }

        // NOW, activate the actual Scrying camera effect
        ScryingSystem.Instance.ActivateScrying(caster);

        movementController.SetMovementLock(false);

        // Tell the skill controller the effect is finished so the lock can be released
        caster.GetComponent<PlayerSkillController>().OnSkillEffectFinished();
    }
}