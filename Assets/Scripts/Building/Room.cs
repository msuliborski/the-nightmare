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
    [SerializeField] private int _areas;
    public int Areas { get { return _areas; } }
    [SerializeField] private int _enemySpawnPointsCounter;
    public int EnemySpawnPointsCounter { get { return _enemySpawnPointsCounter; } }
    private List<GridPoint> _gridPoints = new List<GridPoint>();
    private List<CaptureArea> _captureAreas = new List<CaptureArea>();
    public List<CaptureArea> CaptureAreas { get { return _captureAreas; } }
    private List<GameObject> _enemySpawnPoints = new List<GameObject>();
    public List<GameObject> EnemySpawnPoint { get { return _enemySpawnPoints; } }
    public bool roomCaptured;
    [SerializeField] private bool _hasChest;
    public bool HasChest { get { return _hasChest; } set { _hasChest = value; } }
    
    
    

    private void Start()
    {
        Transform points = transform.GetChild(1);
        for (int i = 0; i < points.childCount; i++)
        {
            _gridPoints.Add(points.GetChild(i).GetComponent<GridPoint>());
        }
        Transform areas = transform.GetChild(3);
        for (int i = 0; i < areas.childCount; i++)
        {
            GameManager.Instance.EnemySpawnPoints.Add(areas.GetChild(i).name, areas.GetChild(i));
            _enemySpawnPoints.Add(areas.GetChild(i).gameObject);
        }
        GameManager.GridRenderes.Add(transform.GetChild(0).gameObject);

        Transform captures = transform.GetChild(2);
        for (int i = 0; i < captures.childCount; i++)
        {
            _captureAreas.Add(captures.GetChild(i).GetComponent<CaptureArea>());
        }
    }

    void Update()
    {
        bool check = true;
        foreach (CaptureArea capture in _captureAreas)
        {
            if (!capture._isCaptured)
            {
                check = false;
                break;
            }
        }

        if (check && !roomCaptured)
        {
            roomCaptured = true;
            foreach (GameObject spawn in _enemySpawnPoints)
            {
                GameManager instance = GameManager.Instance;
                instance.EnemySpawnPoints.Remove(spawn.transform.name);
                spawn.SetActive(false);
            }
        }
        else if (!check && roomCaptured)
        {
            roomCaptured = false;
            foreach (GameObject spawn in _enemySpawnPoints)
            {
                GameManager instance = GameManager.Instance;
                instance.EnemySpawnPoints.Add(spawn.transform.name, spawn.transform);
                spawn.SetActive(true);
            }

        }
    }
    
}
