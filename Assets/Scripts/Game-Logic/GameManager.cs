using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Random = UnityEngine.Random;

public class GameManager : NetworkBehaviour
{


    public static GameManager Instance;
    private List<GameObject> _rooms = new List<GameObject>();
    public List<GameObject> Rooms { get { return _rooms; } }
    private float _prepareTimer = 45f;
    private float[] _timers = { 120f, 180f, 300f };
    private Room _currentRoom;
    public Room CurrentRoom {get { return _currentRoom; }
        set
        {
            _currentRoom = value;
            _enemySpawnPoints.Clear();
            _enemySpawnMarkers.Clear();
            foreach (GameObject spawnPoint in _currentRoom.EnemySpawnPoint)
            {
                _enemySpawnPoints.Add(spawnPoint.transform.name, spawnPoint.transform);
                if (spawnPoint.transform.childCount > 0)
                {
                    CameraFacing cameraFacing = spawnPoint.transform.GetChild(0).GetComponent<CameraFacing>();
                    _enemySpawnMarkers.Add(spawnPoint.transform.GetChild(0).GetComponent<ExPointBlink>());
                    cameraFacing.cameraToLookAt = LocalPlayer.GetComponent<PlayerSetup>().ActionCamera;
                }
            }
            foreach (CaptureArea captureArea in _currentRoom.CaptureAreas)
            {
                Vector3 v = captureArea.transform.position;
                _currentCaptureAreas.Add(v.x.ToString() + "_" + v.y.ToString() + "_" + v.z.ToString(), captureArea);
            }
            
        }
    }
    public static bool IsListeningForReady { get; set; }
    private Dictionary<string,CaptureArea> _currentCaptureAreas = new Dictionary<string, CaptureArea>();
    public Dictionary<string, CaptureArea> CurrentCaptureAreas { get { return _currentCaptureAreas; } }

    private Dictionary<string, Transform> _enemySpawnPoints = new Dictionary<string, Transform>();
    public Dictionary<string, Transform> EnemySpawnPoints { get { return _enemySpawnPoints; } }
    private List<CaptureArea> _captureAreas = new List<CaptureArea>();
    private List<ExPointBlink> _enemySpawnMarkers = new List<ExPointBlink>();
    public List<ExPointBlink> EnemySpawnMarkers { get { return _enemySpawnMarkers; } }
    public List<CaptureArea> CaptureAreas { get { return _captureAreas; } }
    private static List<GameObject> _gridRenderes = new List<GameObject>();



    public static List<GameObject> GridRenderes { get { return _gridRenderes; } }
    private static List<SpriteRenderer> _spriteRenderes = new List<SpriteRenderer>();
    public static List<SpriteRenderer> SpriteRenderer { get { return _spriteRenderes; } }
    [SerializeField] private GameObject _enemyPrefab; 
    [SerializeField] private MatchSettings _matchSettings;
    [SerializeField] private GameObject[] _weapons;
    [SerializeField] private int _waves;
    [SerializeField] private int _enemiesAmount;
    [SyncVar] private int _enemiesCounter = 0;
    [SyncVar] private int _spawnedEnemiesCounter = 0;
    private Coroutine _spawnEnemy; 
    [SerializeField] private List<CameraFacing> _billboards = new List<CameraFacing>();
    public List<CameraFacing> Billboards { get { return _billboards; } set { _billboards = value; } }
    public enum GameState { Building, Fighting }
    //private static GameState _currentState = GameState.Building;
    private static GameState _currentState = GameState.Fighting;
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
                //if (Instance.isServer) Instance.StartCoroutine(Instance.SpawnEnemy());
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
        if (Instance != null) Debug.LogError("More than one GameManager in scene!");
        else Instance = this;
        Transform rooms = GameObject.Find("Rooms").transform;
        for (int i = 0; i < rooms.childCount; i++)
        {
            _rooms.Add(rooms.GetChild(i).gameObject);
            Transform areas = rooms.GetChild(i).GetChild(2);
            
            for(int k = 0; k < areas.childCount; k++)
            {
                Transform area = areas.GetChild(k);
                _spriteRenderes.Add(area.GetComponent<SpriteRenderer>());
                Transform candles = area.GetChild(0);
                for (int j = 0; j < candles.childCount; j++)
                {
                    _billboards.Add(candles.GetChild(j).GetChild(0).GetComponent<CameraFacing>());
                }
            }
        }
        foreach (GameObject room in _rooms)
        {
            Transform points = room.transform.GetChild(1);
            for (int i = 0; i < points.childCount; i++)
            {
                Transform point = points.GetChild(i);
                string posAndTag = point.transform.position.x.ToString() + "_" + point.transform.position.z.ToString() + "_" +  point.tag;
                _buildingPoints.Add(posAndTag, point.GetComponent<GridPoint>());
            }
            Transform captureAreas = room.transform.GetChild(2);
            for (int i = 0; i < captureAreas.childCount; i++)
            {
                Transform captureArea = captureAreas.GetChild(i);
                _captureAreas.Add(captureArea.GetComponent<CaptureArea>());
            }
        }

       _matchSettings.WaitForSpawn -= _matchSettings.EnemyRespawnTime;
    }

    private void Start()
    {
        TurnOnGridRenders(false);
        //if (Instance.isServer) Instance.StartCoroutine(Instance.SpawnEnemy());
    }

    private void Update()
    {
        if (ClockManager.time <= 0 && ClockManager.canCount)
        {
            
            StopHordeAttack();
        }
    }

    #region Building
    private Dictionary<string, GridPoint> _buildingPoints = new Dictionary<string, GridPoint>();
    public Dictionary<string, GridPoint> BuildingPoints { get { return _buildingPoints; } }
    
    public static void TurnOnGridRenders(bool isOn)
    {
        foreach (GameObject renderer in GridRenderes)
            renderer.SetActive(isOn);
        foreach (SpriteRenderer spriteRenderer in SpriteRenderer)
            spriteRenderer.enabled = isOn;
    }

    public static void Win()
    {
        //Debug.Break();
    }

    public static void Lose()
    {
        //Debug.Break();
    }

    [ClientRpc]
    void RpcPauseGame()
    {
        //Debug.Break();
    }
    #endregion

    #region EnemySpawning


    public IEnumerator SpawnEnemy()
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
        GameObject enemy = Instantiate(_enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        NetworkServer.Spawn(enemy);
        _enemiesCounter++;
        _spawnedEnemiesCounter++;
        _spawnEnemy = StartCoroutine(SpawnEnemy());
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

    public void StartHordeAttack()
    {
        ClockManager.time = 60f;
        ClockManager.canCount = true;
        foreach (ExPointBlink exPointBlink in _enemySpawnMarkers)
            exPointBlink.StartBlink();
        if (isServer) _spawnEnemy = StartCoroutine(SpawnEnemy());
    }

    public void StopHordeAttack()
    {
        ClockManager.canCount = false;
        ClockManager.time = 0;
        foreach (ExPointBlink exPointBlink in _enemySpawnMarkers)
            exPointBlink.StopBlink();
        if (isServer) StopCoroutine(_spawnEnemy);
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
        if (!_enemies.ContainsKey(enemyId)) return null;
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

    public void CallCmd(string name)
    {
        CmdRevive(name);
    }
    
    [Command]
    public void CmdRevive(string name)
    {
        RpcRevive(name);
    }

    [ClientRpc]
    private void RpcRevive(string name)
    {
        _players[name].Revive();
    }

    #endregion
}
