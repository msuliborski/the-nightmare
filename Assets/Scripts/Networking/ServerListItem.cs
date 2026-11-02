using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ServerListItem : MonoBehaviour
{
    public delegate void JoinRoomDelegate(LanDiscovery.LanGame game);
    private JoinRoomDelegate _joinRoomCallback;
    [SerializeField] private TextMeshProUGUI _roomNameText;
    private LanDiscovery.LanGame _game;

    public void Setup(LanDiscovery.LanGame game, JoinRoomDelegate joinRoomCallback)
    {
        _game = game;
        _joinRoomCallback = joinRoomCallback;

        string state = "";
        if (!game.IsCompatible) state = " - version " + game.Version;
        else if (game.InProgress) state = " - in progress";
        else if (game.IsFull) state = " - full";
        _roomNameText.text = game.Name + " (" + game.Players + "/" + game.MaxPlayers + ")" + state;

        Button button = GetComponent<Button>();
        if (button != null) button.interactable = game.CanJoin;
    }

    public void JoinRoom()
    {
        if (_game != null && _game.CanJoin) _joinRoomCallback.Invoke(_game);
    }
}
