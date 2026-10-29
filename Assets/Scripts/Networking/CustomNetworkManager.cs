using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.SceneManagement;

// Direct-connect session management. The Unity matchmaker/relay this game used is shut down,
// so a player hosts on their machine (UDP port 7777 by default) and others join by IP address
// or pick the game from the LAN list. Must not declare Awake/OnDestroy/OnValidate: the UNET
// base class uses private ones for its singleton.
public class CustomNetworkManager : NetworkManager
{
    public const int DEFAULT_PORT = 7777;
    public const string DEFAULT_ROOM_NAME = "The Nightmare";
    private const short REJECT_MSG = MsgType.Highest + 1;
    private const string MAIN_MENU_SCENE = "MainMenu";
    private const string LOBBY_SCENE = "LobbyTests";

    public static CustomNetworkManager Instance { get { return singleton as CustomNetworkManager; } }

    // Shown by the lobby after it is (re)loaded, e.g. "Could not connect to ...".
    public static string LastStatus { get; set; }
    public static event Action<string> StatusChanged;

    public string RoomName { get; private set; }
    private bool _wasConnected;
    private bool _leavingOnPurpose;
    private string _rejectReason;
    private static bool _commandLineHandled;
    private List<string> _localAddresses;
    private bool _showOverlay;

    private LanDiscovery _discovery;
    public LanDiscovery Discovery
    {
        get
        {
            if (_discovery == null) _discovery = GetComponent<LanDiscovery>();
            if (_discovery == null) _discovery = gameObject.AddComponent<LanDiscovery>();
            return _discovery;
        }
    }

    public int ConnectedPlayers
    {
        get
        {
            int count = 0;
            foreach (NetworkConnection conn in NetworkServer.connections)
                if (conn != null) count++;
            return count;
        }
    }

    public static bool MatchInProgress
    {
        get
        {
            return GameManager.Instance != null
                   && GameManager.Instance.CurrentMachState != GameManager.MatchState.None
                   && GameManager.Instance.CurrentMachState != GameManager.MatchState.Lobby;
        }
    }

    public bool Host(string roomName, int port)
    {
        if (isNetworkActive) return false;
        StopMatchMaker();
        RoomName = string.IsNullOrEmpty(roomName) ? DEFAULT_ROOM_NAME : roomName.Trim();
        networkPort = port;
        if (StartHost() != null) return true;
        SetStatus("Port " + port + " is busy");
        return false;
    }

    public void Join(string address, int port)
    {
        if (isNetworkActive) return;
        StopMatchMaker();
        networkAddress = address;
        networkPort = port;
        _wasConnected = false;
        _rejectReason = null;
        StartClient();
        SetStatus("Connecting...");
    }

    // "192.168.0.10", "192.168.0.10:7777" or "my-pc.local:7778"
    public static bool TryParseAddress(string input, out string host, out int port)
    {
        host = null;
        port = DEFAULT_PORT;
        if (string.IsNullOrEmpty(input)) return false;
        input = input.Trim();
        int colon = input.LastIndexOf(':');
        if (colon > 0 && input.IndexOf(':') == colon)
        {
            if (!int.TryParse(input.Substring(colon + 1), NumberStyles.Integer, CultureInfo.InvariantCulture, out port)
                || port <= 0 || port > 65535) return false;
            input = input.Substring(0, colon);
        }
        host = input;
        return host.Length > 0;
    }

    // Leave the current game and return to the server lobby.
    public void Leave()
    {
        _leavingOnPurpose = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (NetworkServer.active && NetworkClient.active) StopHost();
        else if (NetworkClient.active) StopClient();
        else if (NetworkServer.active) StopServer();
    }

    public void LeaveToMainMenu()
    {
        string lobby = offlineScene;
        offlineScene = "";
        Leave();
        offlineScene = lobby;
        SceneManager.LoadScene(MAIN_MENU_SCENE);
    }

    private static void SetStatus(string status)
    {
        LastStatus = status;
        if (StatusChanged != null) StatusChanged(status);
    }

    #region Server

    public override void OnStartHost()
    {
        base.OnStartHost();
        _localAddresses = LanDiscovery.GetLocalAddresses();
        Discovery.StartAdvertising(RoomName, networkPort, maxConnections, () => ConnectedPlayers, () => MatchInProgress);
        Debug.Log("[Network] Hosting '" + RoomName + "' on " + string.Join(", ", _localAddresses.ToArray()) + " port " + networkPort);
    }

    public override void OnStopServer()
    {
        Discovery.StopAdvertising();
        base.OnStopServer();
    }

    public override void OnServerConnect(NetworkConnection conn)
    {
        base.OnServerConnect(conn);
        // The host's own local connection is always accepted.
        if (conn.hostId == -1) return;

        string reason = null;
        if (MatchInProgress) reason = "Match already started";
        else if (ConnectedPlayers > maxConnections) reason = "Game is full";
        if (reason != null) StartCoroutine(Reject(conn, reason));
    }

    private IEnumerator Reject(NetworkConnection conn, string reason)
    {
        Debug.Log("[Network] Rejecting connection " + conn.connectionId + ": " + reason);
        conn.Send(REJECT_MSG, new StringMessage(reason));
        yield return new WaitForSecondsRealtime(0.5f); // let the reason arrive before the disconnect
        if (conn.isConnected) conn.Disconnect();
    }

    #endregion

    #region Client

    public override void OnStartClient(NetworkClient client)
    {
        base.OnStartClient(client);
        client.RegisterHandler(REJECT_MSG, msg => _rejectReason = msg.ReadMessage<StringMessage>().value);
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        _wasConnected = true;
        SetStatus("");
        base.OnClientConnect(conn);
    }

    public override void OnClientError(NetworkConnection conn, int errorCode)
    {
        base.OnClientError(conn, errorCode);
        SetStatus("Network error: " + (NetworkError) errorCode);
    }

    // Status is published after base (which stops the client) so listeners see the final state.
    public override void OnClientDisconnect(NetworkConnection conn)
    {
        string status = null;
        if (_rejectReason != null) status = _rejectReason;
        else if (!_wasConnected) status = "Can't reach " + networkAddress;
        else if (!_leavingOnPurpose) status = "Connection lost";
        base.OnClientDisconnect(conn);
        if (status != null) SetStatus(status);
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        _leavingOnPurpose = false;
        _wasConnected = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    #endregion

    #region Command line and overlay

    // The NetworkManager lives in the lobby scene, so jump there when launched with -host / -connect.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void SkipMenusForCommandLine()
    {
        string[] args = Environment.GetCommandLineArgs();
        if (Array.IndexOf(args, "-host") < 0 && Array.IndexOf(args, "-connect") < 0) return;
        if (FindObjectOfType<CustomNetworkManager>() == null) SceneManager.LoadScene(LOBBY_SCENE);
    }

    // TheNightmare -host [-name "Room"] [-port 7777]   or   TheNightmare -connect 192.168.0.10[:7777]
    private void Start()
    {
        if (_commandLineHandled || singleton != this) return;
        _commandLineHandled = true;

        string[] args = Environment.GetCommandLineArgs();
        int port;
        if (!int.TryParse(GetArg(args, "-port"), NumberStyles.Integer, CultureInfo.InvariantCulture, out port))
            port = DEFAULT_PORT;

        if (Array.IndexOf(args, "-host") >= 0)
        {
            Host(GetArg(args, "-name"), port);
            return;
        }

        string connect = GetArg(args, "-connect");
        if (connect == null) return;
        if (connect.IndexOf(':') < 0) connect += ":" + port;
        string address;
        if (TryParseAddress(connect, out address, out port)) Join(address, port);
    }

    private static string GetArg(string[] args, string name)
    {
        int i = Array.IndexOf(args, name);
        return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1)) _showOverlay = !_showOverlay;
    }

    // Small connection panel: always in the pre-match lobby and in the pause menu, or toggled with F1.
    private void OnGUI()
    {
        if (!isNetworkActive) return;
        bool inPreMatchLobby = GameManager.Instance != null && GameManager.Instance.CurrentMachState == GameManager.MatchState.Lobby;
        if (!_showOverlay && !inPreMatchLobby && !PauseGame.menuActive) return;

        string text;
        if (NetworkServer.active)
        {
            if (_localAddresses == null) _localAddresses = LanDiscovery.GetLocalAddresses();
            text = "Hosting \"" + RoomName + "\" (" + ConnectedPlayers + "/" + maxConnections + " players)\n"
                   + "Friends join with: " + string.Join("  or  ", _localAddresses.ToArray())
                   + (networkPort != DEFAULT_PORT ? "  port " + networkPort : "")
                   + "\nOver the internet: forward UDP port " + networkPort + " or use a VPN (Tailscale/ZeroTier).";
        }
        else
        {
            text = "Connected to " + networkAddress + ":" + networkPort
                   + (client != null ? "   ping " + client.GetRTT() + " ms" : "");
        }

        var style = new GUIStyle(GUI.skin.box) { alignment = TextAnchor.UpperLeft, fontSize = 14, wordWrap = true };
        style.normal.textColor = Color.white;
        float width = Mathf.Min(560f, Screen.width - 20f);
        float height = style.CalcHeight(new GUIContent(text), width);
        GUI.Box(new Rect(Screen.width - width - 10f, 10f, width, height), text, style);

        // Clickable while the cursor is free (pause menu / lobby before the welcome screens are closed).
        if (NetworkServer.active && _localAddresses.Count > 0)
        {
            string address = _localAddresses[0] + (networkPort != DEFAULT_PORT ? ":" + networkPort : "");
            if (GUI.Button(new Rect(Screen.width - 130f, height + 15f, 120f, 28f), _copiedUntil > Time.unscaledTime ? "Copied!" : "Copy IP"))
            {
                GUIUtility.systemCopyBuffer = address;
                _copiedUntil = Time.unscaledTime + 1.5f;
            }
        }
    }

    private float _copiedUntil;

    #endregion
}
