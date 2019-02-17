using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerSetup : NetworkBehaviour
{
    [SerializeField] private Behaviour[] _toDisable;
    [SerializeField] private Camera _sceneCamera;
    
    
    // Start is called before the first frame update
    void Start()
    {
        if (!isLocalPlayer)
        {
            DisableComponents();
            AssignRemoteLayer();
        }
        else
        {
           _sceneCamera = Camera.main;
           if (_sceneCamera != null)
                _sceneCamera.gameObject.SetActive(false);
        }

        RegisterPlayer();

    }


    void RegisterPlayer()
    {
        string id = "Player " + GetComponent<NetworkIdentity>().netId;
        transform.name = id;
    }

    private void AssignRemoteLayer()
    {
        gameObject.layer = LayerMask.NameToLayer("RemotePlayer");
    }

    private void DisableComponents()
    {
        for (int i = 0; i < _toDisable.Length; i++)
            _toDisable[i].enabled = false;
    }

    private void OnDisable()
    {
        if (_sceneCamera != null)
            _sceneCamera.gameObject.SetActive(true);
     }
}
