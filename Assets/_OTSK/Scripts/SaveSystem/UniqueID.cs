using System;
using UnityEngine;

public class UniqueID : MonoBehaviour
{
    [SerializeField]
    private string id;
    public string ID => id;

    [ContextMenu("Generate new Unique ID")]
    private void GenerateId()
    {
        id = Guid.NewGuid().ToString();
    }
}