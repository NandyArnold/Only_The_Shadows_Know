// SpectralShiftEffectSO.cs - FINAL VERSION
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "FX_SpectralShift", menuName = "Only The Shadows Know/Skills/Effects/Spectral Shift Effect")]
public class SpectralShiftEffectSO : SkillEffectSO
{
    [Header("Targeting")]
    [SerializeField] private GameObject targetingIndicatorPrefab;
    [SerializeField] private float maxTargetingDistance = 30f;
    [SerializeField] private LayerMask targetingLayerMask;

    public override void Activate(GameObject caster)
    {
        SkillExecutor.Instance.ExecuteCoroutine(TargetingRoutine(caster));
    }

    private IEnumerator TargetingRoutine(GameObject caster)
    {
        // --- SETUP ---
        var playerInput = caster.GetComponent<PlayerInputHandler>();
        var cameraController = caster.GetComponent<CameraController>();

        playerInput.SwitchActionMap("Targeting");
        cameraController.ActivateTargetingCamera();

        TargetingIndicator indicator = Instantiate(targetingIndicatorPrefab).GetComponent<TargetingIndicator>();

        bool isTargeting = true;
        bool confirmed = false;
        Vector3 validTeleportPosition = Vector3.zero;

        Action onConfirm = () => { if (indicator.IsValid) { confirmed = true; isTargeting = false; } };
        Action onCancel = () => isTargeting = false;

        playerInput.OnConfirmInput += onConfirm;
        playerInput.OnCancelInput += onCancel;

        // --- TARGETING LOOP ---
        while (isTargeting)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, maxTargetingDistance, targetingLayerMask))
            {
                indicator.transform.position = hit.point + new Vector3(0, 2, 0); // Offset so projector is above ground
                bool isValid = TeleportManager.Instance.IsValidTeleportLocation(hit.point, out validTeleportPosition);
                indicator.SetValid(isValid);
            }
            else
            {
                indicator.SetValid(false);
            }
            yield return null;
        }

        // --- EXECUTION & CLEANUP ---
        playerInput.OnConfirmInput -= onConfirm;
        playerInput.OnCancelInput -= onCancel;

        if (confirmed)
        {
            var cc = caster.GetComponent<CharacterController>();
            TeleportManager.Instance.ExecuteTeleport(cc, validTeleportPosition);
        }

        Destroy(indicator.gameObject);
        playerInput.SwitchActionMap("Player");
        cameraController.DeactivateTargetingCamera();
    }
}