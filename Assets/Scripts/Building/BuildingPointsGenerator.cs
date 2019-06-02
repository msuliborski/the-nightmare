using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
public class BuildingPointsGenerator : MonoBehaviour
{
    private List<GameObject> _rooms = new List<GameObject>();
    GameObject empty;
    private Canvas _gridCanvas;
    private GameObject _gridPointPrefab;
    private GameObject _areaPrefab;
    


    void Awake()
    {
        _gridCanvas = GameObject.Find("GridCanvas").GetComponent<Canvas>();
        for (int i = 0; i < _gridCanvas.transform.childCount; i++)
        {
            DestroyImmediate(_gridCanvas.transform.GetChild(i).gameObject);
        }
        GameObject roomsGameObject = (GameObject)Resources.Load("Rooms", typeof(GameObject));
        GameObject roomsInstance = Instantiate(roomsGameObject, _gridCanvas.transform);
        roomsInstance.name = "Rooms";
        Transform rooms = roomsInstance.transform;
        for (int i = 0; i < rooms.childCount; i++)
            _rooms.Add(rooms.GetChild(i).gameObject);
        empty = new GameObject();
        _gridPointPrefab = (GameObject)Resources.Load("cross", typeof(GameObject));
        _areaPrefab = (GameObject)Resources.Load("CaptureArea", typeof(GameObject));
        

        
        for (int i = 0; i < _rooms.Count; i++)
        {
            Room room = _rooms[i].GetComponent<Room>();
            int halfWidth = Mathf.FloorToInt(room.Width / 2);
            int halfHeight = Mathf.FloorToInt(room.Height / 2);
            GameObject roomCanvas = Instantiate(empty, room.transform.position, room.transform.rotation, room.transform);
            roomCanvas.name = "RoomRender";
            Vector3 renderPosition = roomCanvas.transform.position;
            
            GameObject points = Instantiate(empty, room.transform.position, room.transform.rotation, room.transform);
            Vector3 pointPosition = points.transform.position;
            points.name = "Points";

            for (float x = room.transform.position.x -halfWidth; x < room.transform.position.x + halfWidth + 1; x += PlacementController.GridTileSize)
                for (float y = room.transform.position.z - halfHeight; y < room.transform.position.z + halfHeight + 1; y += PlacementController.GridTileSize)
                {

                    GameObject point = Instantiate(empty, new Vector3(x, pointPosition.y, y), Quaternion.Euler(0f, 0f, 0f), points.transform);
                    point.tag = room.tag;
                    point.name = "Point " + x + " " + y;
                    point.AddComponent<GridPoint>();
                    GameObject temp = Instantiate(_gridPointPrefab, new Vector3(x, renderPosition.y, y), Quaternion.Euler(90f, 0f, 0f), roomCanvas.transform);
                    point.GetComponent<GridPoint>().setSpriteRenderer(temp);
                }
            GameObject areas = Instantiate(empty, room.transform);
            areas.name = "Areas";
            for (int j = 0; j < room.Areas; j++)
            {
                GameObject area = Instantiate(_areaPrefab, areas.transform);
                area.transform.name = "Area " + j;
                room.CaptureAreas.Add(area.GetComponent<CaptureArea>());
            }
            Transform spawnPoints = Instantiate(empty, room.transform).transform;
            spawnPoints.name = "Enemy Spawn Points";
            for (int j = 0; j < room.EnemySpawnPointsCounter; j++)
            {
                GameObject spawnPoint = Instantiate(empty, spawnPoints);
                spawnPoint.name = "Spawn Point " + j + " Room" + i;
            }
        }
		DestroyImmediate(empty);
        
    }       
  
}
