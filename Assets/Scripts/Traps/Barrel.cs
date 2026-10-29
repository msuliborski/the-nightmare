using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class Barrel : NetworkBehaviour
{
    [SerializeField]
    private GameObject _explosionPrefab;
    private MeshRenderer _renderer;
    private BoxCollider _collider;
    public float damage;
    public string InitialPosAndTag { get; set; }
    private bool _exploded;

    private void Start()
    {
        _collider = GetComponent<BoxCollider>();
        _renderer = GetComponent<MeshRenderer>();
        if (isServer) transform.GetChild(0).GetComponent<BarrelCollider>().server = true;
    }

    // Server only. Two shots, a grenade and a neighbouring barrel can all trigger it in the same
    // frame; it used to explode (and be NetworkServer.Destroy-ed) once per trigger.
    [Server]
    public void Explode()
    {
        if (_exploded) return;
        _exploded = true;
        _collider.enabled = false;
        _renderer.enabled = false;
        transform.GetChild(0).gameObject.SetActive(true);
        RpcExlodeBarrel(InitialPosAndTag);
        StartCoroutine(Decay());
    }

    private IEnumerator Decay()
    {
        yield return new WaitForSeconds(0.2f);
        NetworkServer.Destroy(gameObject);
    }

    [ClientRpc]
    public void RpcExlodeBarrel(string posAndTag)
    {
        GameObject explosion = Instantiate(_explosionPrefab, transform.position, Quaternion.Euler(-90f, 0f, 0f));
        Destroy(explosion, 3f);
        if (!isServer)
        {
            _collider.enabled = false;
            _renderer.enabled = false;
        }

        GridPoint point;
        if (posAndTag != null && GameManager.Instance.BuildingPoints.TryGetValue(posAndTag, out point))
            point.Buildable = true;
    }
}
