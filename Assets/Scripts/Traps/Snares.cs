using UnityEngine;
using UnityEngine.Networking;

public class Snares : NetworkBehaviour
{
    public float freezeTime;
    [SerializeField] private int _enemiesToDestroy = 3;
    public int EnemiesToDestroy { get { return _enemiesToDestroy; } set { _enemiesToDestroy = value; } }
    public int EnemiesCounter { get; set; }
    public string InitialPosAndTag;
    private bool _destroyed;

    private void Awake()
    {
        EnemiesCounter = 0;
    }

    // Server: counts a caught enemy and despawns the snares once used up.
    [Server]
    public void RegisterCaughtEnemy()
    {
        EnemiesCounter++;
        if (EnemiesCounter < EnemiesToDestroy || _destroyed) return;
        _destroyed = true;
        RpcFreeBuildingPoint(InitialPosAndTag);
        NetworkServer.Destroy(gameObject);
    }

    [ClientRpc]
    private void RpcFreeBuildingPoint(string posAndTag)
    {
        GridPoint point;
        if (posAndTag != null && GameManager.Instance.BuildingPoints.TryGetValue(posAndTag, out point))
            point.Buildable = true;
    }
}
