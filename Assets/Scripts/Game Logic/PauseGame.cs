using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

public class PauseGame : MonoBehaviour
{
    public static bool menuActive;
    private GameObject _pauseMenu;
    private NetworkManager _networkManager;
    
    private void Start()
    {
        _networkManager = NetworkManager.singleton;
        _pauseMenu = GameObject.Find("PauseMenu");
        menuActive = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            menuActive = !menuActive;
        }

        _pauseMenu.SetActive(menuActive);
    }

    public void Disconnect()
    {
        MatchInfo info = _networkManager.matchInfo;
        _networkManager.matchMaker.DropConnection(info.networkId, info.nodeId, 0, _networkManager.OnDropConnection);
        //_networkManager.StopHost();
    }
}
