using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;
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
            _currentCaptureAreas.Clear();
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
    private MusicManager _musicManager;
    private Dictionary<string, Transform> _enemySpawnPoints = new Dictionary<string, Transform>();
    private string _nameOfPreviousSpawn;
    public Dictionary<string, Transform> EnemySpawnPoints { get { return _enemySpawnPoints; } }
    private List<CaptureArea> _captureAreas = new List<CaptureArea>();
    private List<ExPointBlink> _enemySpawnMarkers = new List<ExPointBlink>();
    public List<ExPointBlink> EnemySpawnMarkers { get { return _enemySpawnMarkers; } }
    public List<CaptureArea> CaptureAreas { get { return _captureAreas; } }
    private static List<GameObject> _gridRenderes = new List<GameObject>();
    [SerializeField] GameObject[] _doors;
    private CapturePointsUI _cpUI;

    public static List<GameObject> GridRenderes { get { return _gridRenderes; } }
    private static List<SpriteRenderer> _spriteRenderes = new List<SpriteRenderer>();
    public static List<SpriteRenderer> SpriteRenderer { get { return _spriteRenderes; } }
    private GameObject _enemyPrefab;
    [SerializeField] private GameObject[] _enemiesPrefabs;
    [SerializeField] private MatchSettings _matchSettings;
    [SerializeField] private GameObject[] _weapons;
    [SerializeField] private int _waves;
    [SerializeField] private int _enemiesAmount;
    [SyncVar] private int _enemiesCounter = 0;
    [SyncVar] private int _spawnedEnemiesCounter = 0;
    private Coroutine _spawnEnemy; 
    [SerializeField] private List<CameraFacing> _billboards = new List<CameraFacing>();
    public List<CameraFacing> Billboards { get { return _billboards; } set { _billboards = value; } }
    public enum MatchState { None, Lobby, Room1Prepare, Room1Fight, Room2Prepare, Room2Fight, Room3Prepare, Room3Fight, Win, Lose}
    private MatchState _currentMatchState = MatchState.None;
    private WinLoseScreens _screens;

   

    public MatchState CurrentMachState
    {
        get { return _currentMatchState; }
        set
        {
            _currentMatchState = value;
            switch (value)
            {
                case MatchState.Lobby:
                    _arrow.gameObject.SetActive(false);
                    break;

                case MatchState.Room1Prepare:
                    
                    Instance.CurrentRoom = Instance.Rooms[1].GetComponent<Room>();
                    
                    _cpUI.setRoom();
                    _arrow.setTarget();
                    ClockManager.time = 30f;
                    ClockManager.canCount = true;
                    break;
                case MatchState.Room1Fight:
                    _enemyPrefab = _enemiesPrefabs[0];
                    StartHordeAttack();
                    ClockManager.time = _timers[0];
                    ClockManager.canCount = true;
                   
                    break;
                case MatchState.Room2Prepare:
                    
                    Instance.CurrentRoom = Instance.Rooms[2].GetComponent<Room>();
                    _arrow.setTarget();
                    _cpUI.setRoom();
                    _doors[0].SetActive(false);
                    ClockManager.time = 45f;
                    ClockManager.canCount = true;
                    break;

                case MatchState.Room2Fight:
                    _enemyPrefab = _enemiesPrefabs[1];
                    StartHordeAttack();
                    ClockManager.time = _timers[1];
                    ClockManager.canCount = true;
                    break;

                case MatchState.Room3Prepare:
                    
                    Instance.CurrentRoom = Instance.Rooms[0].GetComponent<Room>();
                    _arrow.setTarget();
                    _cpUI.setRoom();
                    _doors[1].SetActive(false);
                    ClockManager.time = 60f;
                    ClockManager.canCount = true;
                    break;

                case MatchState.Room3Fight:
                    _enemyPrefab = _enemiesPrefabs[2];
                    StartHordeAttack();
                    ClockManager.time = _timers[2];
                    ClockManager.canCount = true;
                    break;

                case MatchState.Win:
                    //StopHordeAttack();
                    ClockManager.time = 0f;
                    ClockManager.canCount = false;
                    _screens.ActivateScreen(true);
                    break;

                case MatchState.Lose:
                    //StopHordeAttack();
                    ClockManager.time = 0f;
                    ClockManager.canCount = false;
                    _screens.ActivateScreen(false);
                    break;
            }
        }
    }
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
        _cpUI = GameObject.Find("CapturePoints").GetComponent<CapturePointsUI>();
        foreach (GameObject room in Rooms)
        {
            room.GetComponent<Room>().Setup();
        }
        _musicManager = GetComponent<MusicManager>();
        _screens = GameObject.Find("Win_Lose").GetComponent<WinLoseScreens>();
    }

    private void Start()
    {
        TurnOnGridRenders(false);
        //if (Instance.isServer) Instance.StartCoroutine(Instance.SpawnEnemy());
    }

    private void Update()
    {
        foreach (PlayerManager player in _players.Values)
        {
            if (player._currentHealth <= 0f)
            {
                CurrentMachState = MatchState.Lose;
            }
        }
        
        if (ClockManager.canCount)
        {
            if (ClockManager.time <= 0)
            {
                switch (_currentMatchState)
                {
                    case MatchState.Room1Prepare:
                        CurrentMachState = MatchState.Room1Fight;
                        break;
                    case MatchState.Room1Fight:
                        _musicManager.ChangeClip(false);
                        CurrentMachState = MatchState.Room2Prepare;
                        break;
                    case MatchState.Room2Prepare:
                        _musicManager.ChangeClip(true);
                        CurrentMachState = MatchState.Room2Fight;
                        break;
                    case MatchState.Room2Fight:
                        _musicManager.ChangeClip(false);
                        CurrentMachState = MatchState.Room3Prepare;
                        break;
                    case MatchState.Room3Prepare:
                        _musicManager.ChangeClip(true);
                        CurrentMachState = MatchState.Room3Fight;
                        break;
                    case MatchState.Room3Fight:
                        _musicManager.ChangeClip(false);
                        CurrentMachState = MatchState.Win;
                        break;
                }
            }
            else if (ClockManager.time <= 10f)
            {
                switch (_currentMatchState)
                {
                    case MatchState.Room1Fight:
                        StopHordeAttack();
                        break;

                    case MatchState.Room2Fight:
                        StopHordeAttack();
                        break;

                    case MatchState.Room3Fight:
                        StopHordeAttack();
                        break;
                }
            }
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
        float upperTimeBound;
        switch (_activePlayers.Count)
        {
            case 1:
                upperTimeBound = 9f;
                break;
            case 2:
                upperTimeBound = 7f;
                break;
            case 3:
                upperTimeBound = 5f;
                break;
            default:
                upperTimeBound = 3f;
                break;
        }

        float randTime = Random.Range(1f, upperTimeBound);
        yield return new WaitForSeconds(randTime);
        Transform spawnPoint = null;
        Transform previousSpawn = null;
        if (_nameOfPreviousSpawn != null)
        {
            previousSpawn = EnemySpawnPoints[_nameOfPreviousSpawn];
            EnemySpawnPoints.Remove(_nameOfPreviousSpawn);
        }
        int randIndex = Random.Range(0, EnemySpawnPoints.Keys.Count);
        int counter = 0;
        foreach (KeyValuePair<string, Transform> entry in _enemySpawnPoints)
        {
            if (counter == randIndex)
            {
                //Debug.Log(counter);
                spawnPoint = entry.Value;
                if (_nameOfPreviousSpawn != null) _enemySpawnPoints.Add(_nameOfPreviousSpawn, previousSpawn);
                _nameOfPreviousSpawn = spawnPoint.name;
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

    private static Arrow _arrow;
    private static PlayerManager _localPlayer;
    public static PlayerManager LocalPlayer {
        get { return _localPlayer; }
        set
        {
            _localPlayer = value;
            _arrow = _localPlayer.GetComponentInChildren<Arrow>();
            if (_arrow == null) Debug.Log("honk");
        }
    }
    private static Dictionary<string, PlayerManager> _players = new Dictionary<string, PlayerManager>();
    private static Dictionary<string, PlayerManager> _activePlayers = new Dictionary<string, PlayerManager>();
    public static Dictionary<string, PlayerManager> Players { get { return _players; } }
    public static Dictionary<string, PlayerManager> ActivePlayers { get { return _activePlayers; } }
    public static Dictionary<string, EnemyControllerServer> _enemies = new Dictionary<string, EnemyControllerServer>();
    public static Dictionary<string, EnemyControllerServer> Enemies { get { return _enemies; } }
    private static int _enemyIdCounter = 0;
    public static int EnemyIdCounter { get { return _enemyIdCounter; } set { _enemyIdCounter = value; } }
    
    private static List<int> arrowIndexList = new List<int>() {0, 1, 2, 3};
    public static void RegisterPlayer(string netId, PlayerManager player)
    {
        string playerId = PLAYER_ID_PREFIX + netId;
        _players.Add(playerId, player);
        _activePlayers.Add(playerId, player);
        player.transform.name = playerId;
        
        player.transform.GetChild(4).GetChild(0).gameObject.SetActive(false);
        
        player.transform.GetChild(4).GetChild(arrowIndexList[0]).gameObject.SetActive(true);
        arrowIndexList.Remove(0);
    }

    public static void UnregisterPlayer(string playerId) {
        int index = -1;
        if (_players[playerId].transform.GetChild(4).GetChild(0).gameObject.activeSelf) 
            index = 0;
        else if (_players[playerId].transform.GetChild(4).GetChild(1).gameObject.activeSelf)
            index = 1;
        else if (_players[playerId].transform.GetChild(4).GetChild(1).gameObject.activeSelf)
            index = 2;
        else
            index = 3;
        
        arrowIndexList.Add(index);
        arrowIndexList.Sort();
            
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

    public void Teleport()
    {
        CurrentMachState = MatchState.Room1Prepare;
        Vector3 pos = CurrentRoom.CaptureAreas[0].transform.position;
        float randX = Random.Range(-2f, 2f);
        float randZ = Random.Range(-2f, 2f);
        Rigidbody rigidbody = LocalPlayer.GetComponent<Rigidbody>();
        rigidbody.isKinematic = true;
        LocalPlayer.transform.position = new Vector3(pos.x + randX, pos.y + 1f, pos.z + randZ);
        rigidbody.isKinematic = false;
        TurnOnGridRenders(false);
        PlayerEquipment playerEquipment = LocalPlayer.GetComponentInChildren<PlayerEquipment>();
        if (playerEquipment.Weapon2 != null) Destroy(playerEquipment.Weapon2.gameObject);
        playerEquipment.Weapon1.resetAmmo();
        playerEquipment.Weapon1.gameObject.SetActive(true);
        playerEquipment.Weapon1.transform.localPosition = new Vector3(0.02f, 0.03f, -0.22f);
        playerEquipment.Weapon1.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

        //delete placeable
//        _localPlayer.GetComponent<PlacementController>().placeableCount[0] = 0;
//        _localPlayer.GetComponent<PlacementController>().placeableCount[1] = 0;
//        _localPlayer.GetComponent<PlacementController>().placeableCount[2] = 0;
//        _localPlayer.GetComponent<PlayerShoot>()._grenades = 0;
    }

    #endregion
}
