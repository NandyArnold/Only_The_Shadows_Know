using UnityEngine;
using System;

public class UniqueID : MonoBehaviour
{
    [SerializeField]
    private string _id; // This field is serialized and saved with the scene/prefab

    // A public property to safely read the ID
    public string ID => _id;

    private void Awake()
    {
        // For runtime-spawned prefabs whose ID was cleared in the editor,
        // OR for any scene object that somehow doesn't have an ID yet.
        if (string.IsNullOrEmpty(_id))
        {
            _id = Guid.NewGuid().ToString();
        }
    }

    // A public method for the SaveLoadManager to call when restoring an object.
    // This allows it to set the private _id field from outside.
    public void SetID(string newId)
    {
        _id = newId;
    }
}