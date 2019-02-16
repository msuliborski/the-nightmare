using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerSetup : NetworkBehaviour
{
    [SerializeField] private Behaviour[] toDisable;

    public Camera sceneCamera;
    // Start is called before the first frame update
    void Start()
    {
        if (!isLocalPlayer)
        {
            for (int i = 0; i < toDisable.Length; i++)
                toDisable[i].enabled = false;
        }
        else
        {
           sceneCamera = Camera.main;
           if (sceneCamera != null)
                sceneCamera.gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if (sceneCamera != null)
            sceneCamera.gameObject.SetActive(true);
     }
}
