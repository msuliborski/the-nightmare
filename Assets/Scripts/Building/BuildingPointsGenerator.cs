using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
public class BuildingPointsGenerator : MonoBehaviour
{
    private List<GameObject> _rooms = new List<GameObject>();
    GameObject empty;
    private Canvas _gridCanvas;
    private GameObject _gridPointPrefab;


    void Awake()
    {
        Transform rooms = GameObject.Find("Rooms").transform;
        for (int i = 0; i < rooms.childCount; i++)
            _rooms.Add(rooms.GetChild(i).gameObject);
        empty = new GameObject();
        _gridPointPrefab = (GameObject)Resources.Load("cross", typeof(GameObject));
        _gridCanvas = GameObject.Find("GridCanvas").GetComponent<Canvas>();
        
        for (int i = 0; i < _rooms.Count; i++)
        {
            Room room = _rooms[i].GetComponent<Room>();
            int halfWidth = Mathf.FloorToInt(room.Width / 2);
            int halfHeight = Mathf.FloorToInt(room.Height / 2);
            GameObject roomCanvas = Instantiate(empty, room.transform.position, room.transform.rotation, room.transform);
            roomCanvas.name = "RoomRender";
            GameObject points = Instantiate(empty, room.transform.position, room.transform.rotation, room.transform);
            points.name = "Points";

            for (float x = room.transform.position.x -halfWidth; x < room.transform.position.x + halfWidth + 1; x += PlacementController.GridTileSize)
                for (float y = room.transform.position.z - halfHeight; y < room.transform.position.z + halfHeight + 1; y += PlacementController.GridTileSize)
                {

                    GameObject point = Instantiate(empty, new Vector3(x, 0f, y), Quaternion.Euler(0f, 0f, 0f), points.transform);
                    point.name = "Point " + x + " " + y;
                    point.AddComponent<GridPoint>();
                    GameObject temp = Instantiate(_gridPointPrefab, new Vector3(x, 0.1f, y), Quaternion.Euler(90f, 0f, 0f), roomCanvas.transform);
                    point.GetComponent<GridPoint>().setSpriteRenderer(temp);
                }
        }
		DestroyImmediate(empty);
        
    }       
  
}
