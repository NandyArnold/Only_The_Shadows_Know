using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "FX_BalorsVision", menuName = "Only The Shadows Know/Skills/Effects/Balors Vision Effect")]
public class BalorsVisionEffectSO : SkillEffectSO
{
    [Header("Reveal Settings")]
    [SerializeField] private float revealRadius = 15f;
    [SerializeField] private float revealTickRate = 0.5f; // How often to pulse the reveal
    [SerializeField] private LayerMask revealLayerMask; // Set to everything you want to detect

    [Header("Duration Settings")]
    [SerializeField] private float normalDuration = 3f;
    [SerializeField] private float combatDuration = 1.5f;

    [Header("Time Slow Effect")]
    [SerializeField][Range(0f, 1f)] private float normalTimeScale = 0.25f;
    [SerializeField][Range(0f, 1f)] private float combatTimeScale = 0.75f;

    [Header("Camera & Time Effect")]
    [SerializeField] private float transitionDuration = 0.5f;
    [SerializeField] private float timeScaleAmount = 0.25f;

    private Coroutine _revealCoroutine;
    //  A dictionary to track the active VFX for each revealed entity.
    private Dictionary<RevealableEntity, List<GameObject>> _activeVFX = new Dictionary<RevealableEntity, List<GameObject>>();




    public override void Activate(GameObject caster) { }

    public override IEnumerator StartChannel(GameObject caster, object target = null)
    {
        var cameraController = caster.GetComponent<CameraController>();
        if (cameraController != null)
        {
            cameraController.SwitchToCamera(CameraType.BalorsVision, transitionDuration);
        }

        float timeScaleToUse = CombatManager.Instance.IsPlayerInCombat ? combatTimeScale : normalTimeScale;
        TimeManager.Instance.DoTimeScale(timeScaleToUse, transitionDuration);

        _revealCoroutine = SkillExecutor.Instance.StartCoroutine(RevealRoutine(caster));
        yield break;
    }

    public override void StopChannel(GameObject caster)
    {
        var cameraController = caster.GetComponent<CameraController>();
        if (cameraController != null)
        {
            cameraController.SwitchToCamera(CameraType.Shoulder);
        }
        TimeManager.Instance.ResetTimeScale();

        if (_revealCoroutine != null)
        {
            SkillExecutor.Instance.StopCoroutine(_revealCoroutine);
            _revealCoroutine = null;
        }

        float duration = CombatManager.Instance.IsPlayerInCombat ? combatDuration : normalDuration;
        foreach (var vfxList in _activeVFX.Values)
        {
            foreach (var vfx in vfxList)
            {
                if (vfx != null)
                {
                    // Add the cleanup component and start its timer.
                    vfx.AddComponent<VFXCleanup>().BeginCleanup(duration);
                }
            }
        }
        _activeVFX.Clear();
    }



    private IEnumerator RevealRoutine(GameObject caster)
    {
        while (true)
        {
            Collider[] hits = Physics.OverlapSphere(caster.transform.position, revealRadius, revealLayerMask);

            foreach (var hit in hits)
            {
                if (hit.TryGetComponent<RevealableEntity>(out var entity))
                {
                    if (entity.Type != RevealableType.None)
                    {
                        if (entity.Type != RevealableType.None && !_activeVFX.ContainsKey(entity))
                        {
                            // --- THIS IS THE FIX ---
                            // Call the new CreateOutlineEffect method and store the returned LIST
                            List<GameObject> vfxInstances = VFXManager.Instance.CreateOutlineEffect(entity);
                            if (vfxInstances != null)
                            {
                                _activeVFX.Add(entity, vfxInstances);
                            }
                        }
                    }
                }
            }
            yield return new WaitForSecondsRealtime(revealTickRate);
        }
    }
}