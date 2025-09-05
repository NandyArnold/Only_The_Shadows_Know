// IconMapping.cs
using UnityEngine;
using System;


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

[Serializable]
public class RevealableIconMapping : IconMapping<RevealableType> { }