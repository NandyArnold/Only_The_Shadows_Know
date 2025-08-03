//This is the "cartridge" for the skill. Its only job is to tell the ScryingSystem to activate.
using UnityEngine;

[CreateAssetMenu(fileName = "FX_Scrying", menuName = "Only The Shadows Know/Skills/Effects/Scrying Effect")]
public class ScryingEffectSO : SkillEffectSO
{
    // The caster is the player GameObject.
    public override void Activate(GameObject caster)
    {
        // Tell the ScryingSystem to start, passing in the player
        // so it can find the CameraController.
        ScryingSystem.Instance.ActivateScrying(caster);
    }
}