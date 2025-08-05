using UnityEngine;


public  abstract class DamageTypeSO : ScriptableObject
{
    public abstract float CalculateDamage(float baseValue, GameObject target);
}