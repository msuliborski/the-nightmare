using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
public class BuildingPointsGenerator : MonoBehaviour
{
    GameObject empty;

    void Awake()
    {
        empty = new GameObject();
        
        for (float x = -40f; x < 40f; x += 1f)
            for (float y = -40f; y < 40f; y += 1f)
            {
                GameObject point = Instantiate(empty, new Vector3(x, 0f, y), Quaternion.Euler(0f, 0f, 0f), transform);
                point.name = "Point " + x + " " + y;
            }

        DestroyImmediate(empty);

    }
}
