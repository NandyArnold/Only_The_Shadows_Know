// TargetingIndicator.cs - UPGRADED for Decal Projector
using UnityEngine;
using UnityEngine.Rendering.Universal; // Required for DecalProjector

[RequireComponent(typeof(DecalProjector))]
public class TargetingIndicator : MonoBehaviour
{
    [SerializeField] private Color validColor = Color.cyan;
    [SerializeField] private Color invalidColor = Color.red;
  
    private bool _isValid;
    public bool IsValid => _isValid;

    private DecalProjector _decalProjector;
    private MaterialPropertyBlock _propBlock;
    private int _colorPropertyID;

    private void Awake()
    {
        _decalProjector = GetComponent<DecalProjector>();
        _propBlock = new MaterialPropertyBlock();
        // The property name in the URP Decal Shader Graph is "_BaseColor"
        _colorPropertyID = Shader.PropertyToID("_BaseColor");

        // We need to create a new material instance at runtime to change its color.
        // This is the correct way to handle materials on projectors/decals.
        _decalProjector.material = new Material(_decalProjector.material);
    }

    public void SetValid(bool isValid)
    {
        _isValid = isValid;
        // Set the color property directly on our instanced material.
        _decalProjector.material.SetColor(_colorPropertyID, isValid ? validColor : invalidColor);
    }
}