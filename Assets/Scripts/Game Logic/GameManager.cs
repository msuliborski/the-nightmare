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
    private class SyncListEnemies : SyncListStruct<EnemyStruct> { }
    private SyncListEnemies _enemies;
    [SyncVar] private bool _spawnedByFirstClient = false;
    public MatchSettings MatchSettings { get { return _matchSettings; } set { _matchSettings = value; } }


    void Start()
    {
        if (Instance != null) Debug.LogError("More than one GameManager in scene!");
        else Instance = this;
        if (_enemies == null) _enemies = new SyncListEnemies();
        else foreach (EnemyStruct enemyStruct in _enemies)
            {
                GameObject enemy = enemyStruct.Enemy;
                Instantiate(enemy, enemy.transform.position, enemy.transform.rotation);
            }
        
        StartCoroutine(SpawnEnemy());
    }

    #region EnemySpawning
    
    [Command]
    void CmdSpawnEnemy(int randIndex)
    {
        _spawnedByFirstClient = false;
        RpcSpawnEnemy(randIndex);
    }



    [ClientRpc]
    void RpcSpawnEnemy(int randIndex)
    {
        if (!_spawnedByFirstClient)
            _enemies.Add(new EnemyStruct(Instantiate(_enemyPrefab, _enemySpawnPoints[randIndex])));
        else Instantiate(_enemyPrefab, _enemySpawnPoints[randIndex]);
    }

    private IEnumerator SpawnEnemy()
    {
        yield return new WaitForSeconds(_matchSettings.EnemyRespawnTime);
        int randIndex = Random.Range(0, 3);
        CmdSpawnEnemy(randIndex);
        StartCoroutine(SpawnEnemy());
    }


    #endregion


    #region PlayerTracking

    private const string PLAYER_ID_PREFIX = "Player ";

    private static Dictionary<string, PlayerManager> _players = new Dictionary<string, PlayerManager>();
    public static Dictionary<string, PlayerManager> Players { get { return _players; } }

    public static void RegisterPlayer(string netId, PlayerManager player)
    {
        string playerId = PLAYER_ID_PREFIX + netId;
        _players.Add(playerId, player);
        player.transform.name = playerId;
    }

    public static void UnregisterPlayer(string playerId)
    {
        _players.Remove(playerId);
    }

    public static PlayerManager GetPlayer(string playerId)
    {
        return _players[playerId];
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
