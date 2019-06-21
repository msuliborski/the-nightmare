using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CustomNetworkManager : NetworkManager {
    public List<GameObject> players;

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId) {
        int random = Random.Range(0, players.Count);
        Transform startPosition = GetStartPosition();
        GameObject player = (GameObject) Instantiate(players[random], startPosition.position, Quaternion.identity);
        PlayerManager playerManager = player.GetComponent<PlayerManager>();
        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
    }


    
}