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
        if (_riftPosition == null) // If no rift is placed...
        {
            // ...place a new rift at the player's current position.
            _riftPosition = caster.transform.position;
            _riftInstance = Instantiate(riftMarkerPrefab, _riftPosition.Value, Quaternion.identity);
            Debug.Log($"Rift placed at {_riftPosition.Value}");
            EventManager.Instance.RiftPlaced(_riftPosition.Value, maxTeleportRange);
        }
        else // A rift is already placed, so teleport to it.
        {

            float distanceToRift = Vector3.Distance(caster.transform.position, _riftPosition.Value);
            if (distanceToRift > maxTeleportRange)
            {
                Debug.Log($"Too far to teleport to rift. Distance: {distanceToRift}");
                return; // Exit without teleporting.
            }
            // -----------------------------

            Debug.Log($"Teleporting to rift at {_riftPosition.Value}");
            var cc = caster.GetComponent<CharacterController>();
            TeleportManager.Instance.ExecuteTeleport(cc, _riftPosition.Value);

            CancelRift(); // Clean up the rift after teleporting.
        }
    }

    public static void CancelRift()
    {
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
    public override IEnumerator StartChannel(GameObject caster) { yield break; }
    public override void StopChannel(GameObject caster) { }

}