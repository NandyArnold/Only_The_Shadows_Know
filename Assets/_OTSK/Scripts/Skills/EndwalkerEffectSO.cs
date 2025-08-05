using UnityEngine;

[CreateAssetMenu(fileName = "FX_Endwalker", menuName = "Only The Shadows Know/Skills/Effects/Endwalker Effect")]
public class EndwalkerEffectSO : SkillEffectSO
{
    public override void Activate(GameObject caster)
    {
        if (caster.TryGetComponent<PlayerController>(out var playerController))
        {
            // Toggle the state
            playerController.SetEndwalkerState(!playerController.IsInEndwalkerState);
        }
    }
}