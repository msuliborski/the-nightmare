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


    public void Teleport(PlayerManager playerManager) {
        transform.position = new Vector3(-1.693f, 3.21f, 0f);
        Transform startPosition = GetStartPosition();
        var conn = playerManager.connectionToClient;
        GameObject newPlayer = Instantiate(players[0], startPosition.position, playerManager.transform.rotation);
        PlayerSetup _playerSetup = playerManager.GetComponent<PlayerSetup>();
        foreach (string playerManager1 in GameManager.Players.Keys) {
            Debug.Log(playerManager1);
        }

        GameManager.LocalPlayer = newPlayer.GetComponent<PlayerManager>();
        Debug.Log(playerManager.transform.name);
        GameManager.UnregisterPlayer(playerManager.transform.name);
        GameManager.RegisterPlayer(GameManager.LocalPlayer.netId.ToString(), GameManager.LocalPlayer);
        GameManager.Instance.SetCameraForBillboards(_playerSetup.ActionCamera);
        GameManager.TurnOnGridRenders(false);
        Destroy(playerManager.gameObject);
        NetworkServer.ReplacePlayerForConnection(conn, newPlayer, 0);
    }
}