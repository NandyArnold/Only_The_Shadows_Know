// TacticalIconAtlasSO.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "IconAtlas_New", menuName = "Only The Shadows Know/UI/Tactical Icon Atlas")]
public class TacticalIconAtlasSO : ScriptableObject
{
    [Header("Icon Mappings")]
    [Tooltip("A list that maps a RevealableType to a specific icon sprite and color.")]
    public List<RevealableIconMapping> revealableIcons;

    [Header("Special Icons")]
    [Tooltip("The icon used for enemies that are resistant to scrying.")]
    public Sprite distortedIcon;

    // A single, generic GetIcon method
    public Sprite GetIcon(RevealableType type)
    {
        var mapping = revealableIcons.FirstOrDefault(m => m.type == type);
        return mapping?.icon;
    }

    // A single, generic GetColor method
    public Color GetColor(RevealableType type)
    {
        var mapping = revealableIcons.FirstOrDefault(m => m.type == type);
        return mapping != null ? mapping.iconColor : Color.white;
    }
}