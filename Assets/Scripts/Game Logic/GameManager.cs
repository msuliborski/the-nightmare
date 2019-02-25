using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;
    [SerializeField] private Transform[] _enemySpawnPoints;
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private MatchSettings _matchSettings;
    private struct EnemyStruct
    {
        public GameObject Enemy;
        public EnemyStruct(GameObject enemy) { Enemy = enemy; }
    }
    
    public MatchSettings MatchSettings { get { return _matchSettings; } set { _matchSettings = value; } }


    void Start()
    {
        _matchSettings.WaitForSpawn -= _matchSettings.EnemyRespawnTime;
        if (Instance != null) Debug.LogError("More than one GameManager in scene!");
        else Instance = this;
       if (isServer) StartCoroutine(WaitForSpawn());
    }

    #region EnemySpawning
    
    //[Command]
    //void CmdSpawnEnemy(int randIndex)
    //{
    //   RpcSpawnEnemy(randIndex);
    //}



    //[ClientRpc]
    //void RpcSpawnEnemy(int randIndex)
    //{
    //   Instantiate(_enemyPrefab, _enemySpawnPoints[randIndex]);
    //}


    private IEnumerator WaitForSpawn()
    {
        yield return new WaitForSeconds(_matchSettings.WaitForSpawn);
        StartCoroutine(SpawnEnemy());
    }

    private IEnumerator SpawnEnemy()
    {
        yield return new WaitForSeconds(_matchSettings.EnemyRespawnTime);
        int randIndex = Random.Range(0, 3);
        //CmdSpawnEnemy(randIndex);
        
        NetworkServer.Spawn(Instantiate(_enemyPrefab, _enemySpawnPoints[randIndex]));
        StartCoroutine(SpawnEnemy());
    }


    #endregion


    #region PlayerTracking

    private const string PLAYER_ID_PREFIX = "Player ";

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

    /*void OnGUI()
    {
        GUILayout.BeginArea(new Rect(200, 200, 200, 500));
        GUILayout.BeginVertical();
        foreach (string playerId in _players.Keys)
            GUILayout.Label(playerId + " - " + _players[playerId].transform.name);
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }*/

    #endregion
}
