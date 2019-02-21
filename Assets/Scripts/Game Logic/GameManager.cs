﻿using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private MatchSettings _matchSettings;
    public MatchSettings MatchSettings { get { return _matchSettings } set { _matchSettings = value; } }


    void Awake()
    {
        if (Instance != null) Debug.LogError("More than one GameManager in scene!");
        else Instance = this;
    }  


    #region PlayerTracking

    private const string PLAYER_ID_PREFIX = "Player ";

    private static Dictionary<string, PlayerManager> _players = new Dictionary<string, PlayerManager>();

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
