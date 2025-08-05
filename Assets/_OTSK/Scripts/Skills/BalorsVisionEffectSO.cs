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

    private Coroutine _revealCoroutine;
    //  A dictionary to track the active VFX for each revealed entity.
    private Dictionary<RevealableEntity, GameObject> _activeVFX = new Dictionary<RevealableEntity, GameObject>();



    public override void Activate(GameObject caster) { }

    public override IEnumerator StartChannel(GameObject caster)
    {
        return RevealRoutine(caster);
    }

    public override void StopChannel(GameObject caster)
    {
        if (_revealCoroutine != null) SkillExecutor.Instance.StopCoroutine(_revealCoroutine);

        // When the channel stops, tell all active VFX to begin their cleanup.
        float duration = CombatManager.Instance.IsPlayerInCombat ? combatDuration : normalDuration;
        foreach (var vfxInstance in _activeVFX.Values)
        {
            if (vfxInstance.TryGetComponent<VFXCleanup>(out var cleanup))
            {
                cleanup.BeginCleanup(duration);
            }
        }
        _activeVFX.Clear(); // Clear the tracking dictionary.
    }


    private IEnumerator RevealRoutine(GameObject caster)
    {
        while (true)
        {
            var foundEntities = new HashSet<RevealableEntity>();
            Collider[] hits = Physics.OverlapSphere(caster.transform.position, revealRadius, revealLayerMask);

            foreach (var hit in hits)
            {
                if (hit.TryGetComponent<RevealableEntity>(out var entity))
                {
                    if (entity.type != RevealableType.None)
                    {
                        foundEntities.Add(entity);
                        // If this is a NEW entity we haven't seen before, create a VFX for it.
                        if (!_activeVFX.ContainsKey(entity))
                        {
                            GameObject vfxInstance = VFXManager.Instance.GetRevealEffect(entity.type, entity.transform.position);
                            if (vfxInstance != null)
                            {
                                _activeVFX.Add(entity, vfxInstance);
                            }
                        }
                    }
                }
            }
            yield return new WaitForSeconds(revealTickRate);
        }
    }
}