// IconMapping.cs
using UnityEngine;
using System;

// An enum to categorize different objective types for icon mapping.
public enum ObjectiveType
{
    MainObjective,
    SideObjective,
    ExitPoint,
    HiddenItem,
    // Add any other types you need
}

// A generic, serializable class to link an enum type to a sprite.
[Serializable]
public class IconMapping<T> where T : Enum
{
    public T type;
    public Sprite icon;
    public Color iconColor = Color.white;
}

// Concrete implementations for the Inspector.
[Serializable]
public class EnemyIconMapping : IconMapping<EnemyType> { }

[Serializable]
public class ObjectiveIconMapping : IconMapping<ObjectiveType> { }