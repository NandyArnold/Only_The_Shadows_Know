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

    [Header("Animation Timings")]
    [SerializeField] private float startCastDuration = 0.5f;
    [SerializeField] private float confirmCastDuration = 0.3f;

    [Header("Skill Constraints")]
    [SerializeField] private float maxPlayerTeleportRange = 25f;

    public override void Activate(GameObject caster)
    {
        Debug.Log($"SPECTRAL SHIFT ACTIVATED at frame {Time.frameCount}");
        
    }

    public override IEnumerator StartChannel(GameObject caster)
    {
        return TargetingRoutine(caster);
    }

    // This is the main "orchestrator" coroutine
    private IEnumerator TargetingRoutine(GameObject caster)
    {
        var playerInput = caster.GetComponent<PlayerInputHandler>();
        var cameraController = caster.GetComponent<CameraController>();
        var animController = caster.GetComponent<PlayerAnimationController>();
        var cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
        var skillController = caster.GetComponent<PlayerSkillController>();
        Transform casterEyes = cameraController.transform;
        playerInput.SwitchActionMap("Targeting");



        TargetingIndicator indicator = Instantiate(targetingIndicatorPrefab).GetComponent<TargetingIndicator>();

        bool isTargeting = true;
        bool confirmed = false;
        Vector3 validTeleportPosition = Vector3.zero;
        TargetingMode currentMode = TargetingMode.High;

        Coroutine indicatorUpdateRoutine = skillController.StartCoroutine(
           UpdateIndicatorRoutine(caster, indicator, (targetPos) => validTeleportPosition = targetPos)
       );

        Action onConfirm = () => { if (indicator != null && indicator.IsValid) { confirmed = true; isTargeting = false; } };
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
            // --- START ANIMATION & SETUP (happens in parallel to the indicator moving) ---
            animController.SetSpectralState(1); // 1 = Start Cast
            cameraController.SwitchToCamera(CameraType.Targeting);
            CursorManager.Instance.SetState(CursorState.Targeting);

            yield return new WaitForSeconds(startCastDuration);

            animController.SetSpectralState(2); // 2 = Looping

            // Wait until the player confirms or cancels
            while (isTargeting)
            {
                yield return null;
            }
        }
        finally
        {
            // --- CLEANUP ---
            SkillExecutor.Instance.StopCoroutine(indicatorUpdateRoutine); // Stop the indicator routine
            playerInput.OnConfirmInput -= onConfirm;
            playerInput.OnCancelInput -= onCancel;
            // ... (unsubscribe from onCycleMode)
            if (indicator != null) Destroy(indicator.gameObject);
            cameraController.SwitchToCamera(CameraType.Shoulder);
            CursorManager.Instance.SetState(CursorState.Gameplay);
        }
        // --- FINAL EXECUTION ---
        if (confirmed)
        {
            animController.SetSpectralState(3); // 3 = Play Confirm Cast
            yield return new WaitForSeconds(confirmCastDuration);
            var cc = caster.GetComponent<CharacterController>();
            TeleportManager.Instance.ExecuteTeleport(cc, validTeleportPosition);
        }

        animController.SetSpectralState(0); // Final reset
    }

    // This new coroutine ONLY handles the raycasting and indicator updates.
    private IEnumerator UpdateIndicatorRoutine(GameObject caster, TargetingIndicator indicator, Action<Vector3> onValidPositionFound)
    {
        var cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
        Transform casterEyes = caster.GetComponent<CameraController>().transform;
        var playerInput = caster.GetComponent<PlayerInputHandler>(); // Needed for cycle mode
        TargetingMode currentMode = TargetingMode.High;

        Action<float> onCycleMode = (scrollValue) => {
            if (scrollValue > 0) currentMode = TargetingMode.High;
            else if (scrollValue < 0) currentMode = TargetingMode.Low;
        };
        playerInput.OnCycleTargetingModeInput += onCycleMode;

        while (true)
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
                    if (Physics.Raycast(hit.point + Vector3.up * 0.5f, Vector3.down, out RaycastHit groundHit, 100f, targetingLayerMask))
                    {
                        potentialTarget = groundHit.point;
                        surfaceNormal = groundHit.normal;
                        targetFound = true;
                    }
                }
                else // High mode
                {
                    potentialTarget = hit.point;
                    surfaceNormal = hit.normal;
                    targetFound = true;
                }
            }

            if (targetFound)
            {
                indicator.transform.position = potentialTarget + new Vector3(0, indicatorHeightOffset, 0);

                bool isFlatAndClear = TeleportManager.Instance.IsValidTeleportLocation(potentialTarget, surfaceNormal);
                bool isInRange = Vector3.Distance(caster.transform.position, potentialTarget) <= maxPlayerTeleportRange;
                bool hasLineOfSight = !Physics.Linecast(casterEyes.position, potentialTarget, obstacleLayerMask);

                bool isFinallyValid = isFlatAndClear && isInRange && hasLineOfSight;
                indicator.SetValid(isFinallyValid);

                if (isFinallyValid)
                {
                    onValidPositionFound(potentialTarget);
                }
            }
            else
            {
                indicator.SetValid(false);
            }

            yield return null;
        }

        // This part is unreachable but needed for the lambda to work in some contexts
        // playerInput.OnCycleTargetingModeInput -= onCycleMode;
    }
}

