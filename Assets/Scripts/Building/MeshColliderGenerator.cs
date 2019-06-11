using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class MeshColliderGenerator : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        AddMeshColliderRecursively(transform);
    }
    void AddMeshColliderRecursively(Transform obj)
    {
        if (obj == null) return;
        if (obj.GetComponent<MeshRenderer>() != null) obj.gameObject.AddComponent<MeshCollider>();
        foreach (Transform child in obj.transform)
        {
            if (child == null) continue;
            AddMeshColliderRecursively(child);
        }
    }

}
