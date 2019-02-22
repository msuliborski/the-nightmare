using UnityEngine;
using UnityEngine.Networking;

public class HostGame : MonoBehaviour
{
    [SerializeField] private uint _roomSize = 6;
    private string _roomName;
    private string _password;
    private NetworkManager _networkManager;

    void Start()
    {
        _networkManager = NetworkManager.singleton;
        if (_networkManager.matchMaker == null)
        {
            _networkManager.StartMatchMaker();
        }
    }

    public void SetRoomName(string name)
    {
        _roomName = name;
    }

    public void SetPassword(string password)
    {
        _password = password;
    }

    public void CreateRoom()
    {
        if (_roomName != "" && _roomName != null)
        {
            Debug.Log("Creating Room: "+_roomName+" for "+_roomSize+" players");
            _networkManager.matchMaker.CreateMatch(_roomName, _roomSize, true, _password, "", "", 0, 0, _networkManager.OnMatchCreate);
            GameObject.Find("Lobby").SetActive(false);
        }
    }
}
