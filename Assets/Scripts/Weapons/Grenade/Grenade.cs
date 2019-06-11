using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Grenade : NetworkBehaviour
{
    [SerializeField] private float _secsToExplosion;
    [SerializeField] private float _decayTime;
    public float _damage;
    [SerializeField] GameObject _explosionPrefab;
    private MeshRenderer _renderer;
    private CapsuleCollider _collider;

    void Start()
    {
        _collider = GetComponent<CapsuleCollider>();
        _renderer = GetComponent<MeshRenderer>();
        StartCoroutine(Explode());
        if (isServer) transform.GetChild(0).GetComponent<GrenadeCollider>().server = true;
    }

    private IEnumerator Explode()
    {
        yield return new WaitForSeconds(_secsToExplosion);
        _collider.enabled = false;
        _renderer.enabled = false;
        transform.GetChild(0).gameObject.SetActive(true);
        if (isServer) RpcExlode();
        StartCoroutine(Decay());
    }

    private IEnumerator Decay()
    {
        yield return new WaitForSeconds(_decayTime);
        Destroy(gameObject);
        
    }

    [ClientRpc]
    void RpcExlode()
    {
        GameObject explosion = Instantiate(_explosionPrefab, transform.position + 0.8f * Vector3.up, transform.rotation);
        Destroy(explosion, 0.87f);
    }

}
