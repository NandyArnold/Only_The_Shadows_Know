// SpectralShiftEffectSO.cs - FINAL ROBUST VERSION
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using System.Collections.Generic;

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

    public override IEnumerator StartChannel(GameObject caster, object target = null)
    {
        return TargetingRoutine(caster);
    }

    // This is the main "orchestrator" coroutine
    private IEnumerator TargetingRoutine(GameObject caster)
    {
        var playerInput = caster.GetComponent<PlayerInputHandler>();
        var cameraController = caster.GetComponent<CameraController>();
        var animController = caster.GetComponent<PlayerAnimationController>();
        var skillController = caster.GetComponent<PlayerSkillController>();

        TargetingIndicator indicator = null;
        Coroutine indicatorUpdateRoutine = null;
        bool isTargeting = true;
        bool confirmed = false;
        Vector3 validTeleportPosition = Vector3.zero;
        TargetingMode currentMode = TargetingMode.High;

        Action onConfirm = () => {
            if (indicator != null && indicator.IsValid)
            {
                confirmed = true;
                isTargeting = false;
            }
            else
            {
                // Optional: Play a "fail" sound or show a UI message
                Debug.Log("Confirm failed: Target not valid.");
            }
        };
        Action onCancel = () => isTargeting = false;
        Action<float> onCycleMode = (scrollValue) => {
            if (scrollValue > 0) currentMode = TargetingMode.High;
            else if (scrollValue < 0) currentMode = TargetingMode.Low;
        };

        playerInput.OnConfirmInput += onConfirm;
        playerInput.OnCancelInput += onCancel;
        playerInput.OnCycleTargetingModeInput += onCycleMode;

        try
        {
            Debug.Log("TargetingRoutine: START. Entering TRY block.");
            animController.SetSpectralState(1);
            cameraController.SwitchToCamera(CameraType.Targeting);
            CursorManager.Instance.SetState(CursorState.Targeting);

            Debug.Log("TargetingRoutine: Waiting for startCastDuration.");
            yield return new WaitForSeconds(startCastDuration);

            Debug.Log("TargetingRoutine: Finished startCastDuration wait.");
            animController.SetSpectralState(2);
            indicator = Instantiate(targetingIndicatorPrefab).GetComponent<TargetingIndicator>();

            indicatorUpdateRoutine = skillController.StartCoroutine(UpdateIndicatorRoutine(caster, indicator, () => currentMode, (targetPos) => validTeleportPosition = targetPos));

            while (isTargeting)
            {
                yield return null;
            }
        }
        finally
        {
            Debug.Log("TargetingRoutine: FINALLY block executed.");
            // CLEANUP: This block now ONLY handles cleanup. It does NOT call CancelChannel.
            if (indicatorUpdateRoutine != null) skillController.StopCoroutine(indicatorUpdateRoutine);

            playerInput.OnConfirmInput -= onConfirm;
            playerInput.OnCancelInput -= onCancel;
            playerInput.OnCycleTargetingModeInput -= onCycleMode;

            //skillController.OnSkillEffectFinished();
            CursorManager.Instance.SetState(CursorState.Gameplay);
            //if (indicator != null) Destroy(indicator.gameObject);
            animController.SetSpectralState(0);

        }

        if (confirmed)
        {
            Debug.Log("TargetingRoutine: Target CONFIRMED. Playing state 3.");
            animController.SetSpectralState(3);
            yield return new WaitForSeconds(confirmCastDuration);

            caster.GetComponent<PlayerStats>().ConsumeMana(
           caster.GetComponent<PlayerSkillController>().GetSkill(SkillIdentifier.SpectralShift).manaCost
       );

            var cc = caster.GetComponent<CharacterController>();
            TeleportManager.Instance.ExecuteTeleport(cc, validTeleportPosition);
        }
        else
        {
            Debug.Log("TargetingRoutine: Target NOT confirmed (cancelled).");
        }
        Debug.Log("TargetingRoutine: Coroutine finished.");


        if (indicator != null) Destroy(indicator.gameObject);
        //cameraController.SwitchToCamera(CameraType.Shoulder);
        //CursorManager.Instance.SetState(CursorState.Gameplay);
        //animController.SetSpectralState(0);
        skillController.OnSkillEffectFinished();
            cameraController.SwitchToCamera(CameraType.Shoulder);


    }


    // This new coroutine ONLY handles the raycasting and indicator updates.
    private IEnumerator UpdateIndicatorRoutine(GameObject caster, TargetingIndicator indicator, Func<TargetingMode> getCurrentMode, Action<Vector3> onValidPositionFound)
    {
        var cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
        Transform casterEyes = caster.GetComponent<CameraController>().transform;                            

        while (true)
        {
            TargetingMode currentMode = getCurrentMode();
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

