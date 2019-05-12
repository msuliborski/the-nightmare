
using UnityEngine;

public class GridPoint : MonoBehaviour
{
    [SerializeField] private bool _buildable = true;
    public bool Buildable { get { return _buildable; } set { _buildable = value; } }
}
