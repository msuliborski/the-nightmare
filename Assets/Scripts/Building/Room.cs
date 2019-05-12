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


}
