using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

// Finds games on the local network with plain UDP broadcasts, replacing the shut-down
// Unity matchmaker. A host advertises once a second; the lobby listens and lists hosts.
// Works the same on Windows, macOS and Linux and does not depend on UNET.
public class LanDiscovery : MonoBehaviour
{
    public const int DISCOVERY_PORT = 47777;
    private const string MAGIC = "NIGHTMARE/1";
    private const float ADVERTISE_INTERVAL = 1f;
    private const float GAME_TIMEOUT = 3.5f;

    public class LanGame
    {
        public string Name;
        public string Address;
        public int Port;
        public int Players;
        public int MaxPlayers;
        public bool InProgress;
        public string Version;
        public float LastSeen;

        public bool IsFull { get { return Players >= MaxPlayers; } }
        public bool IsCompatible { get { return Version == Application.version; } }
        public bool CanJoin { get { return IsCompatible && !IsFull && !InProgress; } }
    }

    public event Action GamesChanged;
    public IEnumerable<LanGame> Games { get { return _games.Values; } }
    public bool IsAdvertising { get { return _advertiser != null; } }
    public bool IsListening { get { return _listener != null; } }

    private readonly Dictionary<string, LanGame> _games = new Dictionary<string, LanGame>();
    private UdpClient _listener;
    private UdpClient _advertiser;
    private List<IPEndPoint> _broadcastTargets;
    private Func<string> _buildAdvertisement;
    private float _nextAdvertiseTime;

    public void StartAdvertising(string roomName, int gamePort, int maxPlayers, Func<int> players, Func<bool> inProgress)
    {
        StopAdvertising();
        try
        {
            _advertiser = new UdpClient();
            _advertiser.EnableBroadcast = true;
            _broadcastTargets = GetBroadcastTargets();
            string safeName = (roomName ?? "").Replace('|', '/');
            _buildAdvertisement = () => string.Join("|", new[]
            {
                MAGIC,
                gamePort.ToString(CultureInfo.InvariantCulture),
                players().ToString(CultureInfo.InvariantCulture),
                maxPlayers.ToString(CultureInfo.InvariantCulture),
                inProgress() ? "1" : "0",
                Application.version.Replace('|', '/'),
                safeName
            });
            _nextAdvertiseTime = 0f;
        }
        catch (Exception e)
        {
            Debug.LogWarning("[LanDiscovery] Cannot advertise on LAN: " + e.Message);
            StopAdvertising();
        }
    }

    public void StopAdvertising()
    {
        if (_advertiser != null) _advertiser.Close();
        _advertiser = null;
        _buildAdvertisement = null;
    }

    public void StartListening()
    {
        if (_listener != null) return;
        try
        {
            // Address reuse lets several game instances on one machine listen at once (handy for testing).
            _listener = new UdpClient();
            _listener.ExclusiveAddressUse = false;
            _listener.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _listener.Client.Bind(new IPEndPoint(IPAddress.Any, DISCOVERY_PORT));
        }
        catch (Exception e)
        {
            Debug.LogWarning("[LanDiscovery] Cannot listen for LAN games: " + e.Message);
            StopListening();
        }
    }

    public void StopListening()
    {
        if (_listener != null) _listener.Close();
        _listener = null;
        if (_games.Count > 0)
        {
            _games.Clear();
            if (GamesChanged != null) GamesChanged();
        }
    }

    private void Update()
    {
        if (_advertiser != null && Time.unscaledTime >= _nextAdvertiseTime)
        {
            _nextAdvertiseTime = Time.unscaledTime + ADVERTISE_INTERVAL;
            Advertise();
        }

        if (_listener != null)
        {
            bool changed = Receive();
            changed |= ExpireOldGames();
            if (changed && GamesChanged != null) GamesChanged();
        }
    }

    private void Advertise()
    {
        byte[] data = Encoding.UTF8.GetBytes(_buildAdvertisement());
        foreach (IPEndPoint target in _broadcastTargets)
        {
            try { _advertiser.Send(data, data.Length, target); }
            catch (Exception) { /* an interface went down; the other targets still work */ }
        }
    }

    private bool Receive()
    {
        bool changed = false;
        try
        {
            while (_listener.Available > 0)
            {
                IPEndPoint sender = null;
                byte[] data = _listener.Receive(ref sender);
                LanGame game = Parse(Encoding.UTF8.GetString(data), sender.Address);
                if (game == null) continue;

                string key = game.Address + ":" + game.Port;
                LanGame known;
                if (_games.TryGetValue(key, out known) && known.Name == game.Name && known.Players == game.Players
                    && known.InProgress == game.InProgress)
                {
                    known.LastSeen = game.LastSeen;
                    continue;
                }
                // A host on this machine is heard both via loopback and its LAN address; keep the LAN one.
                if (IPAddress.IsLoopback(sender.Address))
                {
                    if (HasNonLoopbackEntry(game)) continue;
                }
                else
                {
                    _games.Remove(IPAddress.Loopback + ":" + game.Port);
                }
                _games[key] = game;
                changed = true;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("[LanDiscovery] Receive failed: " + e.Message);
        }
        return changed;
    }

    private bool HasNonLoopbackEntry(LanGame game)
    {
        foreach (LanGame g in _games.Values)
            if (g.Port == game.Port && g.Name == game.Name && g.Address != game.Address
                && !IPAddress.IsLoopback(IPAddress.Parse(g.Address))) return true;
        return false;
    }

    private bool ExpireOldGames()
    {
        List<string> expired = null;
        foreach (KeyValuePair<string, LanGame> entry in _games)
        {
            if (Time.unscaledTime - entry.Value.LastSeen <= GAME_TIMEOUT) continue;
            if (expired == null) expired = new List<string>();
            expired.Add(entry.Key);
        }
        if (expired == null) return false;
        foreach (string key in expired) _games.Remove(key);
        return true;
    }

    private static LanGame Parse(string message, IPAddress sender)
    {
        string[] parts = message.Split(new[] { '|' }, 7);
        if (parts.Length != 7 || parts[0] != MAGIC) return null;

        int port, players, maxPlayers;
        if (!int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out port)) return null;
        if (!int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out players)) return null;
        if (!int.TryParse(parts[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out maxPlayers)) return null;

        return new LanGame
        {
            Port = port,
            Players = players,
            MaxPlayers = maxPlayers,
            InProgress = parts[4] == "1",
            Version = parts[5],
            Name = parts[6],
            Address = sender.ToString(),
            LastSeen = Time.unscaledTime
        };
    }

    private static List<IPEndPoint> GetBroadcastTargets()
    {
        var targets = new List<IPEndPoint>
        {
            new IPEndPoint(IPAddress.Broadcast, DISCOVERY_PORT),
            new IPEndPoint(IPAddress.Loopback, DISCOVERY_PORT)
        };
        // 255.255.255.255 only leaves through the default interface on some systems (notably macOS),
        // so also send to each interface's directed broadcast address.
        try
        {
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus != OperationalStatus.Up) continue;
                foreach (UnicastIPAddressInformation info in nic.GetIPProperties().UnicastAddresses)
                {
                    if (info.Address.AddressFamily != AddressFamily.InterNetwork || IPAddress.IsLoopback(info.Address)) continue;
                    if (info.IPv4Mask == null) continue;
                    byte[] ip = info.Address.GetAddressBytes();
                    byte[] mask = info.IPv4Mask.GetAddressBytes();
                    for (int i = 0; i < 4; i++) ip[i] = (byte) (ip[i] | ~mask[i]);
                    var target = new IPEndPoint(new IPAddress(ip), DISCOVERY_PORT);
                    if (!targets.Contains(target)) targets.Add(target);
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log("[LanDiscovery] Could not enumerate network interfaces: " + e.Message);
        }
        return targets;
    }

    // Best guess of the addresses other players should type to reach this machine.
    public static List<string> GetLocalAddresses()
    {
        var result = new List<string>();
        try
        {
            // Connecting a UDP socket sends nothing; it only asks the OS which interface routes outward.
            using (var probe = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                probe.Connect("8.8.8.8", 53);
                result.Add(((IPEndPoint) probe.LocalEndPoint).Address.ToString());
            }
        }
        catch (Exception) { /* offline; fall back to the interface list */ }

        try
        {
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus != OperationalStatus.Up) continue;
                foreach (UnicastIPAddressInformation info in nic.GetIPProperties().UnicastAddresses)
                {
                    if (info.Address.AddressFamily != AddressFamily.InterNetwork || IPAddress.IsLoopback(info.Address)) continue;
                    string address = info.Address.ToString();
                    if (!result.Contains(address)) result.Add(address);
                }
            }
        }
        catch (Exception) { /* not supported on this platform */ }

        if (result.Count == 0) result.Add("127.0.0.1");
        return result;
    }

    private void OnDestroy()
    {
        StopAdvertising();
        StopListening();
    }
}
