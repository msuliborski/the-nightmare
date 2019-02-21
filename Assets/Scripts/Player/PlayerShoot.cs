using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerShoot : NetworkBehaviour
{
    
    
    public Camera Cam { get; set; }

    public PlayerEquipment Equipment { get; set; }


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
        Equipment.WeaponSound.Play();
        CmdPlayerShooting(transform.name);
        RaycastHit hit;
        Debug.Log(Equipment.Weapon.Range);
        if (Physics.Raycast(Cam.transform.position, Cam.transform.forward, out hit, Equipment.Weapon.Range, _mask))
        {
            Debug.Log("We hit " + hit.collider.name);
            if (hit.collider.tag == "Player")
                CmdPlayerShoot(hit.collider.transform.parent.transform.parent.name, Equipment.Weapon.Damage);
        }
    }


    [Command]
    void CmdPlayerShooting(string shootingPlayerId)
    {
        GameManager.GetPlayer(shootingPlayerId).GetComponent<PlayerEquipment>().RpcPlayerShooting();
    }

    

    [Command]
    void CmdPlayerShoot(string shootPlayerId, int damage)
    {
        Debug.Log(shootPlayerId + " has been shoot");
        GameManager.GetPlayer(shootPlayerId).RpcTakeDamage(damage);
    }
}
