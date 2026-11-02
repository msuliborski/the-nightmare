using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

// Sent by the server just before it refuses a connection, so the joining player sees why.
public struct RejectMessage : NetworkMessage
{
    public string reason;
}

// Direct-connect session management on top of Mirror. A player hosts on their machine (UDP port 7777
// by default, KCP transport) and others join by IP address or pick the game from the LAN list.
public class CustomNetworkManager : NetworkManager
{
    public const int DEFAULT_PORT = 7777;
    public const string DEFAULT_ROOM_NAME = "The Nightmare";
    private const string MAIN_MENU_SCENE = "MainMenu";
    private const string LOBBY_SCENE = "LobbyTests";

    public static CustomNetworkManager Instance { get { return singleton as CustomNetworkManager; } }

    // Shown by the lobby after it is (re)loaded, e.g. "Can't reach ...".
    public static string LastStatus { get; set; }
    public static event Action<string> StatusChanged;

    public string RoomName { get; private set; }
    private bool _wasConnected;
    private bool _leavingOnPurpose;
    private string _rejectReason;
    private static bool _commandLineHandled;
    private List<string> _localAddresses;
    private bool _showOverlay;
    private float _copiedUntil;

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

    // The game port lives on the transport (KCP) in Mirror.
    public int NetworkPort
    {
        get
        {
            var portTransport = transport as PortTransport;
            return portTransport != null ? portTransport.Port : DEFAULT_PORT;
        }
        set
        {
            var portTransport = transport as PortTransport;
            if (portTransport != null) portTransport.Port = (ushort) Mathf.Clamp(value, 1, 65535);
        }
    }

    public int ConnectedPlayers { get { return NetworkServer.connections.Count; } }

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
        RoomName = string.IsNullOrEmpty(roomName) ? DEFAULT_ROOM_NAME : roomName.Trim();
        NetworkPort = port;
        try
        {
            StartHost();
        }
        catch (Exception e)
        {
            Debug.LogWarning("[Network] Could not host: " + e.Message);
        }
        if (NetworkServer.active) return true;
        SetStatus("Port " + port + " is busy");
        return false;
    }

    public void Join(string address, int port)
    {
        if (isNetworkActive) return;
        networkAddress = address;
        NetworkPort = port;
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
        switch (mode)
        {
            case NetworkManagerMode.Host: StopHost(); break;
            case NetworkManagerMode.ClientOnly: StopClient(); break;
            case NetworkManagerMode.ServerOnly: StopServer(); break;
        }
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
        Discovery.StartAdvertising(RoomName, NetworkPort, maxConnections, () => ConnectedPlayers, () => MatchInProgress);
        Debug.Log("[Network] Hosting '" + RoomName + "' on " + string.Join(", ", _localAddresses.ToArray()) + " port " + NetworkPort);
    }

    public override void OnStopServer()
    {
        Discovery.StopAdvertising();
        base.OnStopServer();
    }

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        base.OnServerConnect(conn);
        // The host's own local connection is always accepted.
        if (conn is LocalConnectionToClient) return;

        string reason = null;
        if (MatchInProgress) reason = "Match already started";
        else if (ConnectedPlayers > maxConnections) reason = "Game is full";
        if (reason != null) StartCoroutine(Reject(conn, reason));
    }

    private IEnumerator Reject(NetworkConnectionToClient conn, string reason)
    {
        Debug.Log("[Network] Rejecting connection " + conn.connectionId + ": " + reason);
        conn.Send(new RejectMessage { reason = reason });
        yield return new WaitForSecondsRealtime(0.5f); // let the reason arrive before the disconnect
        if (NetworkServer.connections.ContainsKey(conn.connectionId)) conn.Disconnect();
    }

    #endregion

    #region Client

    public override void OnStartClient()
    {
        base.OnStartClient();
        NetworkClient.RegisterHandler<RejectMessage>(msg => _rejectReason = msg.reason, false);
    }

    public override void OnClientConnect()
    {
        _wasConnected = true;
        SetStatus("");
        base.OnClientConnect();
    }

    public override void OnClientError(TransportError error, string reason)
    {
        base.OnClientError(error, reason);
        SetStatus("Network error: " + error);
    }

    public override void OnClientDisconnect()
    {
        string status = null;
        if (_rejectReason != null) status = _rejectReason;
        else if (!_wasConnected) status = "Can't reach " + networkAddress;
        else if (!_leavingOnPurpose) status = "Connection lost";
        base.OnClientDisconnect();
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
        if (FindFirstObjectByType<CustomNetworkManager>() == null) SceneManager.LoadScene(LOBBY_SCENE);
    }

    // TheNightmare -host [-name "Room"] [-port 7777]   or   TheNightmare -connect 192.168.0.10[:7777]
    public override void Start()
    {
        base.Start();
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

    public override void Update()
    {
        base.Update();
        if (Input.GetKeyDown(KeyCode.F1)) _showOverlay = !_showOverlay;
    }

    // Small connection panel: always in the pre-match lobby and in the pause menu, or toggled with F1.
    private void OnGUI()
    {
        if (!isNetworkActive) return;
        bool inPreMatchLobby = GameManager.Instance != null && GameManager.Instance.CurrentMachState == GameManager.MatchState.Lobby;
        if (!_showOverlay && !inPreMatchLobby && !PauseGame.menuActive) return;

        int port = NetworkPort;
        string text;
        if (NetworkServer.active)
        {
            if (_localAddresses == null) _localAddresses = LanDiscovery.GetLocalAddresses();
            text = "Hosting \"" + RoomName + "\" (" + ConnectedPlayers + "/" + maxConnections + " players)\n"
                   + "Friends join with: " + string.Join("  or  ", _localAddresses.ToArray())
                   + (port != DEFAULT_PORT ? "  port " + port : "")
                   + "\nOver the internet: forward UDP port " + port + " or use a VPN (Tailscale/ZeroTier).";
        }
        else
        {
            text = "Connected to " + networkAddress + ":" + port
                   + "   ping " + Math.Round(NetworkTime.rtt * 1000) + " ms";
        }

        var style = new GUIStyle(GUI.skin.box) { alignment = TextAnchor.UpperLeft, fontSize = 14, wordWrap = true };
        style.normal.textColor = Color.white;
        float width = Mathf.Min(560f, Screen.width - 20f);
        float height = style.CalcHeight(new GUIContent(text), width);
        GUI.Box(new Rect(Screen.width - width - 10f, 10f, width, height), text, style);

        // Clickable while the cursor is free (pause menu / lobby before the welcome screens are closed).
        if (NetworkServer.active && _localAddresses.Count > 0)
        {
            string address = _localAddresses[0] + (port != DEFAULT_PORT ? ":" + port : "");
            if (GUI.Button(new Rect(Screen.width - 130f, height + 15f, 120f, 28f), _copiedUntil > Time.unscaledTime ? "Copied!" : "Copy IP"))
            {
                GUIUtility.systemCopyBuffer = address;
                _copiedUntil = Time.unscaledTime + 1.5f;
            }
        }
    }

    #endregion
}
