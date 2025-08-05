// SkillEffectSO.cs
using System.Collections;
using UnityEngine;

/// <summary>
/// An abstract ScriptableObject that defines the contract for a skill's effect.
/// All concrete skill behaviors will inherit from this class.
/// </summary>
public abstract class SkillEffectSO : ScriptableObject
{
    // The main method that gets called to activate the skill's logic.
    // We pass the "caster" so the effect knows who is performing the skill.
    public abstract void Activate(GameObject caster);
    public virtual IEnumerator StartChannel(GameObject caster)
    {
        yield break;
    }
    public virtual void StopChannel(GameObject caster) { }
}