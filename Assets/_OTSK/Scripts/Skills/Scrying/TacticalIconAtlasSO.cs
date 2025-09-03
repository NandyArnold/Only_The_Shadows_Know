// TacticalIconAtlasSO.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "IconAtlas_New", menuName = "Only The Shadows Know/UI/Tactical Icon Atlas")]
public class TacticalIconAtlasSO : ScriptableObject
{
    [Header("Entity Icons")]
    public List<EnemyIconMapping> enemyIcons;
    public List<ObjectiveIconMapping> objectiveIcons;

    [Header("Special Icons")]
    [Tooltip("The icon used for enemies that are resistant to scrying.")]
    public Sprite distortedIcon;

    public Sprite GetIcon(EnemyType type)
    {
        // Find the mapping for the given enemy type and return its icon.
        // If not found, FirstOrDefault returns null.
        return enemyIcons.FirstOrDefault(mapping => mapping.type == type)?.icon;
    }

    public Sprite GetIcon(ObjectiveType type)
    {
        // Find the mapping for the given objective type and return its icon.
        return objectiveIcons.FirstOrDefault(mapping => mapping.type == type)?.icon;
    }

    public Color GetColor(EnemyType type)
    {
        // Find the mapping and return its color.
        // If not found, return white.
        var mapping = enemyIcons.FirstOrDefault(m => m.type == type);
        return mapping != null ? mapping.iconColor : Color.white;
    }

    public Color GetColor(ObjectiveType type)
    {
        var mapping = objectiveIcons.FirstOrDefault(m => m.type == type);
        return mapping != null ? mapping.iconColor : Color.white;
    }
}