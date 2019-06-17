using UnityEngine;
using UnityEngine.Networking;

public class Snares : NetworkBehaviour
{
    public float freezeTime;
    [SerializeField] private int _enemiesToDestroy = 3;
    public int EnemiesToDestroy { get { return _enemiesToDestroy; } set { _enemiesToDestroy = value; } }
    public int EnemiesCounter { get; set; }
    public string InitialPosAndTag;

    private void Awake()
    {
        EnemiesCounter = 0;
    }

    [ClientRpc]
    public void RpcDestroySnares(string posAndTag)
    {
        GameManager.Instance.BuildingPoints[posAndTag].Buildable = true;
        Destroy(gameObject);
    }
}
