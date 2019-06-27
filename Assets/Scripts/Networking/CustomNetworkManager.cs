using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CustomNetworkManager : NetworkManager {
    public List<GameObject> players;

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId) {
        int random = Random.Range(0, players.Count);
//        Transform startPosition = GetStartPosition();
        GameObject g = GameObject.Find("LobbySpawn");
        if (g == null) Debug.Log("g to null");
        else Debug.Log("g to nie null");
        Vector3 lobbySpawn = GameObject.Find("LobbySpawn").transform.position;
        Vector3 randomStartPosition = new Vector3(lobbySpawn.x + Random.Range(-1, 1), lobbySpawn.y, lobbySpawn.z + Random.Range(-1, 1));
        GameObject player = (GameObject) Instantiate(players[random], randomStartPosition, Quaternion.identity);
        PlayerManager playerManager = player.GetComponent<PlayerManager>();
        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
    }


    
}