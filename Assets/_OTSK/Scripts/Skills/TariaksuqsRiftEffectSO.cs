// Create this new script: TariaksuqsRiftEffectSO.cs
using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "FX_TariaksuqsRift", menuName = "Only The Shadows Know/Skills/Effects/Tariaksuqs Rift Effect")]
public class TariaksuqsRiftEffectSO : SkillEffectSO
{
    [SerializeField] private GameObject riftMarkerPrefab;

    // Static variables hold the state across all uses of this skill.
    private static Vector3? _riftPosition = null;
    private static GameObject _riftInstance;

    public static bool IsRiftActive => _riftPosition.HasValue;

    public override void Activate(GameObject caster)
    {
        if (_riftPosition == null) // If no rift is placed...
        {
            // ...place a new rift at the player's current position.
            _riftPosition = caster.transform.position;
            _riftInstance = Instantiate(riftMarkerPrefab, _riftPosition.Value, Quaternion.identity);
            Debug.Log($"Rift placed at {_riftPosition.Value}");
        }
        else // A rift is already placed, so teleport to it.
        {
            Debug.Log($"Teleporting to rift at {_riftPosition.Value}");
            var cc = caster.GetComponent<CharacterController>();
            TeleportManager.Instance.ExecuteTeleport(cc, _riftPosition.Value);

            CancelRift(); // Clean up the rift after using it.
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
        Debug.Log("Rift cancelled.");
    }

    // These are not used for this instant-cast skill
    public override IEnumerator StartChannel(GameObject caster) { yield break; }
    public override void StopChannel(GameObject caster) { }
}