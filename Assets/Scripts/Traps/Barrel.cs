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
    private BoxCollider _collider;
    public float damage;
    public string InitialPosAndTag { get; set; }

    private void Start()
    {
        _collider = GetComponent<BoxCollider>();
        _renderer = GetComponent<MeshRenderer>();
        if (isServer) transform.GetChild(0).GetComponent<BarrelCollider>().server = true;
    }

    public void Explode()
    {
        _collider.enabled = false;
        _renderer.enabled = false;
        transform.GetChild(0).gameObject.SetActive(true);
        Debug.Log("EEEEE");
        gameObject.GetComponent<NetworkIdentity>().AssignClientAuthority(this.GetComponent<NetworkIdentity>().connectionToClient);
        CmdExplodeBarrel(InitialPosAndTag);
        StartCoroutine(Decay());
    }
    
    private IEnumerator Decay()
    {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
        
    }

    [Command]
    public void CmdExplodeBarrel(string posAndTag)
    {
        Debug.Log("IIIII");
        RpcExlodeBarrel(posAndTag);
    }

    [ClientRpc]
    public void RpcExlodeBarrel(string posAndTag)
    {
        Debug.Log("AAAAA");
        GameObject explosion = Instantiate(_explosionPrefab, transform.position, Quaternion.Euler(-90f, 0f, 0f));
        Destroy(explosion, 3f);
        GameManager.Instance.BuildingPoints[posAndTag].Buildable = true;
    }

    
}
