using System;
using UnityEngine;

[DisallowMultipleComponent]
public class UniqueID : MonoBehaviour
{
    [SerializeField] // Make the ID visible in the Inspector
    private string _id;
    public string ID => _id;

    private void Awake()
    {
        // Generate the ID if it doesn't already have one (for scene objects)
        if (string.IsNullOrEmpty(_id))
        {
            _id = Guid.NewGuid().ToString();
        }
    }
}