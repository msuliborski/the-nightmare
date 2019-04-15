using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimScaler : MonoBehaviour
{
    private Transform parent;
    private Vector3 scale;
    
    void Start()
    {
        scale = transform.localScale;
        parent = transform.parent;
    }

    void Update()
    {
        transform.localScale = new Vector3 (scale.x/parent.transform.localScale.x,scale.y/parent.transform.localScale.y,scale.z/parent.transform.localScale.z);
    }
}
