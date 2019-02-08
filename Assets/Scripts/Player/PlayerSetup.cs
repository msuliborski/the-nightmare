using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerSetup : NetworkBehaviour
{
    [SerializeField] private Behaviour[] toDisable;

    private Camera tempCamera;
    // Start is called before the first frame update
    void Start()
    {
        
        if (!isLocalPlayer)
        {
            for (int i = 0; i < toDisable.Length; i++)
            {
                toDisable[i].enabled = false;
            }
        }
        else
        {
            tempCamera = Camera.main;
            if (tempCamera != null)
            {
                tempCamera.gameObject.SetActive(false);
            }
        }
    }

    private void OnDisable()
    {
        if (tempCamera != null)
        {
            tempCamera.gameObject.SetActive(true);
        }
        
    }
}
