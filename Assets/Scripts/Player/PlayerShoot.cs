using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerShoot : NetworkBehaviour
{
    
    public AudioSource WeaponSound { get; set; }
    public PlayerWeapon Weapon { get; set; }
    public Camera Cam { get; set; }

    [SerializeField] private LayerMask _mask;
    
    // Start is called before the first frame update
    void Start()
    {
        if (Cam == null) enabled = false;
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
            Shoot();
    }

    void Shoot()
    {
        WeaponSound.Play();
        RaycastHit hit;
        if (Physics.Raycast(Cam.transform.position, Cam.transform.forward, out hit, Weapon.Range, _mask))
        {
            Debug.Log("We hit " + hit.collider.name);
            if (hit.collider.tag == "Player")
                CmdPlayerShoot(hit.collider.name);
        }
    }

    [Command]
    void CmdPlayerShoot(string playerId)
    {
        Debug.Log(playerId + " has been shoot");
    }
}
