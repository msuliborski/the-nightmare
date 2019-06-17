using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Barrel : NetworkBehaviour
{
    [SerializeField]
    private GameObject _explosionPrefab;
    private MeshRenderer _renderer;
    private CapsuleCollider _collider;

    private void Start()
    {
        _collider = GetComponent<CapsuleCollider>();
        _renderer = GetComponent<MeshRenderer>();    }

    public void Explode()
    {
        _collider.enabled = false;
        _renderer.enabled = false;
        transform.GetChild(0).gameObject.SetActive(true);
        if(isServer) RpcExlode();
        StartCoroutine(Decay());
    }
    
    private IEnumerator Decay()
    {
        yield return new WaitForSeconds(0.2f);
        Destroy(gameObject);
        
    }
    
    [ClientRpc]
    void RpcExlode()
    {
        GameObject explosion = Instantiate(_explosionPrefab, transform.position, transform.rotation);
        Destroy(explosion, 3f);
    }
}
