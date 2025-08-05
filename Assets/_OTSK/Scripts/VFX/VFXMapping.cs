// a small helper class to link a RevealableType to a VFX prefab in the Inspector.

using UnityEngine;

[System.Serializable]
public class VFXMapping
{
    public RevealableType type;
    public GameObject vfxPrefab;
}