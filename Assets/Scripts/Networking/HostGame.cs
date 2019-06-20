using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class HostGame : MonoBehaviour
{
    [SerializeField] private uint _roomSize = 4;
    [SerializeField] private GameObject _panel;
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
            _panel.SetActive(true);
            _panel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Creating room " + _roomName + "...";
            _networkManager.matchMaker.CreateMatch(_roomName, _roomSize, true, "", "", "", 0, 0, _networkManager.OnMatchCreate);
        }
    }
}
