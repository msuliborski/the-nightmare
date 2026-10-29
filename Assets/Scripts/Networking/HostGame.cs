using TMPro;
using UnityEngine;

public class HostGame : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    private string _roomName;

    public void SetRoomName(string name)
    {
        _roomName = name;
    }

    // Kept for the existing (hidden) password field binding; direct connections have no password.
    public void SetPassword(string password)
    {
    }

    public void CreateRoom()
    {
        CustomNetworkManager manager = CustomNetworkManager.Instance;
        if (manager == null || manager.isNetworkActive) return;

        string roomName = string.IsNullOrEmpty(_roomName) ? CustomNetworkManager.DEFAULT_ROOM_NAME : _roomName;
        _panel.SetActive(true);
        _panel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Creating room " + roomName + "...";
        if (!manager.Host(roomName, CustomNetworkManager.DEFAULT_PORT))
            _panel.SetActive(false);
    }
}
