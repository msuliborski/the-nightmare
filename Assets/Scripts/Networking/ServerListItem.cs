using TMPro;
using UnityEngine;
using UnityEngine.Networking.Match;
using UnityEngine.UI;

public class ServerListItem : MonoBehaviour
{
    public delegate void JoinRoomDelegate(MatchInfoSnapshot match);
    private JoinRoomDelegate _joinRoomCallback; 
    [SerializeField] private TextMeshProUGUI _roomNameText;
    private MatchInfoSnapshot _match;

    public void Setup(MatchInfoSnapshot match, JoinRoomDelegate joinRoomCallback)
    {
        _match = match;
        _joinRoomCallback = joinRoomCallback;
        
        _roomNameText.text = _match.name + " (" + _match.currentSize + "/" + _match.maxSize + ")";
    }

    public void JoinRoom()
    {
        _joinRoomCallback.Invoke(_match);
    }
}
