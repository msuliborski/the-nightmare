using UnityEngine;


[ExecuteInEditMode]
public class BuildingPointsGenerator : MonoBehaviour
{
    GameObject empty;
    private Canvas _gridCanvas;
    private GameObject _gridPointPrefab;

    void Awake()
    {
        empty = new GameObject();
        _gridPointPrefab = (GameObject)Resources.Load("cross", typeof(GameObject));
        _gridCanvas = GameObject.Find("GridCanvas").GetComponent<Canvas>();
        for (float x = -40f; x < 40f; x += PlacementController.GridTileSize)
            for (float y = -40f; y < 40f; y += PlacementController.GridTileSize)
            {
                GameObject point = Instantiate(empty, new Vector3(x, 0f, y), Quaternion.Euler(0f, 0f, 0f), transform);
                point.name = "Point " + x + " " + y;
                Instantiate(_gridPointPrefab, new Vector3(x + 0.5f, 0.1f, y + 0.5f), Quaternion.Euler(90f, 0f, 0f), _gridCanvas.transform);
            }
       
		DestroyImmediate(empty);

    }
  
}
