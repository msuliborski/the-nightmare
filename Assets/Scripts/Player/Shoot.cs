using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;

public class Shoot : NetworkBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask mask;
    
    // Start is called before the first frame update
    void Start()
    {
        if (cam == null)
        {
            this.enabled = false;
        }
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            shoot();
        }
    }

    void shoot()
    {
        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, 100f, mask))
        {
            Debug.Log("We hit "+hit.collider.name);
        }
    }
}
