// TacticalIconAtlasSO.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "IconAtlas_New", menuName = "Only The Shadows Know/UI/Tactical Icon Atlas")]
public class TacticalIconAtlasSO : ScriptableObject
{
    [Header("Specific Mappings")]
    public List<EnemyIconMapping> enemyIcons;
    public List<ObjectiveIconMapping> objectiveIcons;

    [Header("Generic & Special Icons")]
    public List<RevealableIconMapping> genericIcons;
    [Tooltip("The icon used for enemies that are resistant to scrying.")]
    public Sprite distortedIcon;
    [Tooltip("The icon used if no other specific or generic mapping is found.")]
    public Sprite defaultIcon;
    [Tooltip("The icon used to represent a corpse.")]
    public Sprite corpseIcon;

    [Header("Map Edge Indicators")] 
    public Sprite mainObjectiveArrow;
    public Sprite sideObjectiveArrow;
    public Sprite exitPointArrow;
    public Sprite defaultObjectiveArrow;

    // A single, generic GetIcon method
    public Sprite GetIcon(EnemyType type)
    {
        var mapping = enemyIcons.FirstOrDefault(m => m.type == type);
        // If no specific enemy icon is found, return the default icon
        return mapping?.icon ?? defaultIcon;
    }

    public Sprite GetIcon(ObjectiveType type)
    {
        var mapping = objectiveIcons.FirstOrDefault(m => m.type == type);
        return mapping?.icon ?? defaultIcon;
    }

    public Sprite GetIcon(RevealableType type)
    {
        var mapping = genericIcons.FirstOrDefault(m => m.type == type);
        return mapping?.icon ?? defaultIcon;
    }

   
    public Color GetColor(RevealableType type)
    {
        var mapping = genericIcons.FirstOrDefault(m => m.type == type);
        return mapping != null ? mapping.iconColor : Color.white;
    }
    public Color GetColor(EnemyType type)
    {
        var mapping = enemyIcons.FirstOrDefault(m => m.type == type);
       
        return mapping != null ? mapping.iconColor : Color.white;
    }

    public Color GetColor(ObjectiveType type)
    {
        var mapping = objectiveIcons.FirstOrDefault(m => m.type == type);
        return mapping != null ? mapping.iconColor : Color.white;
    }

  
}