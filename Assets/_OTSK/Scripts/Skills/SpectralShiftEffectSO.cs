// SpectralShiftEffectSO.cs - CORRECTED with Mouse Aiming
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine; // Required for Cinemachine Brain

[CreateAssetMenu(fileName = "FX_SpectralShift", menuName = "Only The Shadows Know/Skills/Effects/Spectral Shift Effect")]
public class SpectralShiftEffectSO : SkillEffectSO
{
    [Header("Targeting")]
    [SerializeField] private GameObject targetingIndicatorPrefab;
    [SerializeField] private float maxTargetingDistance = 100f;
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
        var cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();

        playerInput.SwitchActionMap("Targeting");
        cameraController.ActivateTargetingCamera();
        CursorManager.Instance.SetState(CursorState.Targeting);

        TargetingIndicator indicator = Instantiate(targetingIndicatorPrefab).GetComponent<TargetingIndicator>();

        bool isTargeting = true;
        bool confirmed = false;
        Vector3 validTeleportPosition = Vector3.zero;

        Action onConfirm = () => { if (indicator.IsValid) { confirmed = true; isTargeting = false; } };
        Action onCancel = () => isTargeting = false;

        playerInput.OnConfirmInput += onConfirm;
        playerInput.OnCancelInput += onCancel;

        // This try...finally block ensures the "finally" code ALWAYS runs.
        try
        {
            // --- TARGETING LOOP ---
            while (isTargeting)
            {
                Camera activeCamera = cinemachineBrain.OutputCamera;
                Ray ray = activeCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

                if (Physics.Raycast(ray, out RaycastHit hit, maxTargetingDistance, targetingLayerMask))
                {
                    indicator.transform.position = hit.point + new Vector3(0, 3f, 0);
                    bool isValid = TeleportManager.Instance.IsValidTeleportLocation(hit.point, out validTeleportPosition);
                    indicator.SetValid(isValid);
                }
                else
                {
                    indicator.SetValid(false);
                }
                yield return null;
            }
        }
        finally
        {
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
            CursorManager.Instance.SetState(CursorState.Gameplay);
        }
    }
}