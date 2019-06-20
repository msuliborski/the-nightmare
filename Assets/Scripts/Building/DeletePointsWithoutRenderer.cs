using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DeletePointsWithoutRenderer : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        foreach (Transform child in transform)
        {
            GridPoint gridPoint = child.GetComponent<GridPoint>();
            if (gridPoint.getSpriteRenderer() == null) DestroyImmediate(child.gameObject); 
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
