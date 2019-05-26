using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    private List<GameObject> _buildingPoints = new List<GameObject>();

    [SerializeField] private int _width;
    public int Width { get { return _width; } }
    [SerializeField] private int _height;
    public int Height { get { return _height; } }
    private List<GridPoint> _gridPoints = new List<GridPoint>();

    private void Start()
    {
        Transform points = transform.GetChild(1);
        for (int i = 0; i < points.childCount; i++)
        {
            _gridPoints.Add(points.GetChild(i).GetComponent<GridPoint>());
        }
    }

}
