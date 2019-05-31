using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class GameManager : NetworkBehaviour
{

    public static GameManager Instance;
    private List<GameObject> _rooms = new List<GameObject>();
    public List<GameObject> Rooms { get { return _rooms; } }
    private Dictionary<string, Transform> _enemySpawnPoints = new Dictionary<string, Transform>();
    public Dictionary<string, Transform> EnemySpawnPoints { get { return _enemySpawnPoints; } }
    private List<CaptureArea> _captureAreas = new List<CaptureArea>();
    public List<CaptureArea> CaptureAreas { get { return _captureAreas; } }
    private static List<GameObject> _gridRenderes = new List<GameObject>();
    public static List<GameObject> GridRenderes { get { return _gridRenderes; } }
    [SerializeField] private GameObject _enemyPrefab; 
    [SerializeField] private MatchSettings _matchSettings;
    [SerializeField] private GameObject[] _weapons;
    [SerializeField] private int _waves;
    [SerializeField] private int _enemiesAmount;
    [SyncVar] private int _enemiesCounter = 0;
    [SyncVar] private int _spawnedEnemiesCounter = 0;
    [SerializeField] private CameraFacing[] _billboards;
    public CameraFacing[] Billboards { get { return _billboards; } set { _billboards = value; } }
    public enum GameState { Building, Fighting }
    private static GameState _currentState = GameState.Building;
    //private static GameState _currentState = GameState.Fighting;
    public static GameState CurrentState
    {
        get { return _currentState; }
        set
        {
            if (value == GameState.Building)
            {
                foreach (PlayerManager player in _players.Values)
                    player.SetBuildingMode();
                if (Instance.isServer) Instance.StopCoroutine(Instance.SpawnEnemy());
                TurnOnGridRenders(true);
            }
            else if (value == GameState.Fighting)
            {
                foreach (PlayerManager player in _players.Values)
                    player.SetActionMode();
                TurnOnGridRenders(false);
                if (Instance.isServer) Instance.StartCoroutine(Instance.SpawnEnemy());
            }
            _currentState = value;
        }
    }
    [SyncVar] public int ReadyPlayersCnt = 0;            
    public GameObject[] Weapons { get { return _weapons; } set { _weapons = value; } }
    public MatchSettings MatchSettings { get { return _matchSettings; } set { _matchSettings = value; } }
    [SerializeField] private GameObject[] _floorsToDisable;
    public GameObject[] FloorsToDisable { get { return _floorsToDisable; }  set { _floorsToDisable = value; } }

    void Awake()
    {
        Transform rooms = GameObject.Find("Rooms").transform;
        for (int i = 0; i < rooms.childCount; i++)
            _rooms.Add(rooms.GetChild(i).gameObject);

        foreach (GameObject room in _rooms)
        {
            Transform points = room.transform.GetChild(1);
            for (int i = 0; i < points.childCount; i++)
            {
                Transform point = points.GetChild(i);
                _buildingPoints.Add(new Vector2(point.transform.position.x, point.transform.position.z), point.GetComponent<GridPoint>());
            }
            Transform captureAreas = room.transform.GetChild(2);
            for (int i = 0; i < captureAreas.childCount; i++)
            {
                Transform captureArea = captureAreas.GetChild(i);
                _captureAreas.Add(captureArea.GetComponent<CaptureArea>());
            }
        }

       _matchSettings.WaitForSpawn -= _matchSettings.EnemyRespawnTime;
        if (Instance != null) Debug.LogError("More than one GameManager in scene!");
        else Instance = this;
     }


    #region Building
    private Dictionary<Vector2, GridPoint> _buildingPoints = new Dictionary<Vector2, GridPoint>();
    public Dictionary<Vector2, GridPoint> BuildingPoints { get { return _buildingPoints; } }
    
    public static void TurnOnGridRenders(bool isOn)
    {
        foreach (GameObject renderer in GridRenderes)
            renderer.SetActive(isOn);
    }
    
    
    #endregion

    #region EnemySpawning


    private IEnumerator SpawnEnemy()
    {
        yield return new WaitForSeconds(_matchSettings.EnemyRespawnTime);
        Transform spawnPoint = null;
        int randIndex = Random.Range(0, EnemySpawnPoints.Keys.Count);
        int counter = 0;
        foreach (KeyValuePair<string, Transform> entry in _enemySpawnPoints)
        {
            if (counter == randIndex)
            {
                //Debug.Log(counter);
                spawnPoint = entry.Value;
                break;
            }
            counter++;
        }
        NetworkServer.Spawn(Instantiate(_enemyPrefab, spawnPoint));
        _enemiesCounter++;
        _spawnedEnemiesCounter++;
        StartCoroutine(SpawnEnemy());
    }


    public static void SetLayerRecursively(GameObject obj, string layerName)
    {
        if (obj == null) return;

        obj.layer = LayerMask.NameToLayer(layerName);

        foreach (Transform child in obj.transform)
        {
            if (child == null) continue;
            SetLayerRecursively(child.gameObject, layerName);
        }
    }


    #endregion


    #region PlayerAndEnemies

    private const string PLAYER_ID_PREFIX = "Player ";

    public static PlayerManager LocalPlayer { get; set; }
    private static Dictionary<string, PlayerManager> _players = new Dictionary<string, PlayerManager>();
    private static Dictionary<string, PlayerManager> _activePlayers = new Dictionary<string, PlayerManager>();
    public static Dictionary<string, PlayerManager> Players { get { return _players; } }
    public static Dictionary<string, PlayerManager> ActivePlayers { get { return _activePlayers; } }
    public static Dictionary<string, EnemyControllerServer> _enemies = new Dictionary<string, EnemyControllerServer>();
    public static Dictionary<string, EnemyControllerServer> Enemies { get { return _enemies; } }
    private static int _enemyIdCounter = 0;
    public static int EnemyIdCounter { get { return _enemyIdCounter; } set { _enemyIdCounter = value; } }

    public static void RegisterPlayer(string netId, PlayerManager player)
    {
        string playerId = PLAYER_ID_PREFIX + netId;
        _players.Add(playerId, player);
        _activePlayers.Add(playerId, player);
        player.transform.name = playerId;
    }

    public static void UnregisterPlayer(string playerId)
    {
        _players.Remove(playerId);
        if (_activePlayers.ContainsKey(playerId)) _activePlayers.Remove(playerId);
    }

    public static PlayerManager GetPlayer(string playerId)
    {
        if (!_players.ContainsKey(playerId)) return null;
        return _players[playerId];
    }

    public static EnemyControllerServer GetEnemy(string enemyId)
    {
        return _enemies[enemyId];
    }

    public static void DeactivatePlayer(string playerId)
    {
        _activePlayers.Remove(playerId);
    }

    public static void ActivatePlayer(string playerId, PlayerManager player)
    {
        _activePlayers.Add(playerId, player);
    }

    public void SetCameraForBillboards(Camera cam)
    {
        foreach (CameraFacing cameraFacing in _billboards)
            cameraFacing.cameraToLookAt = cam;
    }

    #endregion
}
