// SpectralShiftEffectSO.cs - FINAL ROBUST VERSION
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

[CreateAssetMenu(fileName = "FX_SpectralShift", menuName = "Only The Shadows Know/Skills/Effects/Spectral Shift Effect")]
public class SpectralShiftEffectSO : SkillEffectSO
{
    private enum TargetingMode { High, Low }

    [Header("Targeting")]
    [SerializeField] private GameObject targetingIndicatorPrefab;
    [SerializeField] private float indicatorHeightOffset = 3f; // NEW: Configurable offset
    [SerializeField] private float maxTargetingDistance = 100f;
    [SerializeField] private LayerMask targetingLayerMask;
    [SerializeField] private LayerMask obstacleLayerMask;

    [Header("Skill Constraints")]
    [SerializeField] private float maxPlayerTeleportRange = 25f;

    public override void Activate(GameObject caster)
    {
        SkillExecutor.Instance.ExecuteCoroutine(TargetingRoutine(caster));
    }

    private IEnumerator TargetingRoutine(GameObject caster)
    {
        var playerInput = caster.GetComponent<PlayerInputHandler>();
        var cameraController = caster.GetComponent<CameraController>();
        var cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
        Transform casterEyes = cameraController.transform;

        playerInput.SwitchActionMap("Targeting");
        cameraController.ActivateTargetingCamera();
        CursorManager.Instance.SetState(CursorState.Targeting);

        TargetingIndicator indicator = Instantiate(targetingIndicatorPrefab).GetComponent<TargetingIndicator>();

        bool isTargeting = true;
        bool confirmed = false;
        Vector3 validTeleportPosition = Vector3.zero;
        TargetingMode currentMode = TargetingMode.High;

        Action onConfirm = () => { if (indicator.IsValid) { confirmed = true; isTargeting = false; } };
        Action onCancel = () => isTargeting = false;
        Action<float> onCycleMode = (scrollValue) => {
            if (scrollValue > 0) currentMode = TargetingMode.High;
            else if (scrollValue < 0) currentMode = TargetingMode.Low;
            Debug.Log("Targeting Mode: " + currentMode);
        };

        playerInput.OnConfirmInput += onConfirm;
        playerInput.OnCancelInput += onCancel;
        playerInput.OnCycleTargetingModeInput += onCycleMode;

        try
        {
            while (isTargeting)
            {
                Camera activeCamera = cinemachineBrain.OutputCamera;
                Ray ray = activeCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

                Vector3 potentialTarget = Vector3.zero;
                Vector3 surfaceNormal = Vector3.up;
                bool targetFound = false;

                if (Physics.Raycast(ray, out RaycastHit hit, maxTargetingDistance, targetingLayerMask | obstacleLayerMask))
                {
                    if (currentMode == TargetingMode.Low)
                    {
                        // In Low mode, we cast down from the hit point to find the ground.
                        if (Physics.Raycast(hit.point + Vector3.up * 0.5f, Vector3.down, out RaycastHit groundHit, 100f, targetingLayerMask))
                        {
                            potentialTarget = groundHit.point;
                            surfaceNormal = groundHit.normal; // Use the ground's normal
                            targetFound = true;
                        }
                    }
                    else // High mode
                    {
                        // In High mode, we use the surface we hit directly.
                        potentialTarget = hit.point;
                        surfaceNormal = hit.normal; // Use the surface's normal
                        targetFound = true;
                    }
                }

                if (targetFound)
                {
                    indicator.transform.position = potentialTarget + new Vector3(0, indicatorHeightOffset, 0);

                    // --- RUN ALL VALIDATION CHECKS ---
                    bool isFlatAndClear = TeleportManager.Instance.IsValidTeleportLocation(potentialTarget, surfaceNormal);
                    bool isInRange = Vector3.Distance(caster.transform.position, potentialTarget) <= maxPlayerTeleportRange;
                    bool hasLineOfSight = !Physics.Linecast(casterEyes.position, potentialTarget, obstacleLayerMask);

                    bool isFinallyValid = isFlatAndClear && isInRange && hasLineOfSight;
                    indicator.SetValid(isFinallyValid);

                    if (isFinallyValid)
                    {
                        validTeleportPosition = potentialTarget;
                    }
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
            // --- CLEANUP ---
            playerInput.OnConfirmInput -= onConfirm;
            playerInput.OnCancelInput -= onCancel;
            playerInput.OnCycleTargetingModeInput -= onCycleMode;

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