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
        
        
        if (Input.GetButtonDown("Fire1") && Equipment.Weapon.State == PlayerWeapon.WeaponState.idle)
            Shoot();
        
        //add reload
    }

    void Shoot()
    {
        Equipment.PlayerShooting();
        Equipment.Weapon.shoot();
        //Cam.transform.localEulerAngles = new Vector3(Cam.transform.localEulerAngles.x + 2f, 0f, 0f);
        CmdPlayerShooting();
        RaycastHit hit;
        if (Physics.Raycast(Cam.transform.position, Cam.transform.forward, out hit, Equipment.Weapon.Range, _mask))
        {
            Debug.Log("We hit " + hit.collider.name);
            if (hit.collider.tag == "Player")
                CmdPlayerShoot(hit.collider.GetComponentInParent<PlayerManager>().transform.name, Equipment.Weapon.Damage);
            else if (hit.collider.tag == "EnemyHead")
                CmdEnemyShoot(hit.collider.GetComponentInParent<EnemyController>().transform.name, 3 * Equipment.Weapon.Damage);
            else if (hit.collider.tag == "EnemyBody")
                CmdEnemyShoot(hit.collider.GetComponentInParent<EnemyController>().transform.name, 2 * Equipment.Weapon.Damage);
            else if (hit.collider.tag == "EnemyLegs")
                CmdEnemyShoot(hit.collider.GetComponentInParent<EnemyController>().transform.name, Equipment.Weapon.Damage);

            Equipment.DoHitEffect(hit.point, hit.normal);
            CmdOnHit(hit.point, hit.normal);
        }
    }


    [Command]
    void CmdPlayerShooting()
    {
        Equipment.RpcPlayerShooting();
    }

    [Command]
    void CmdOnHit(Vector3 hitPoint, Vector3 normal)
    {
        Equipment.RpcDoHitEffect(hitPoint, normal);
    }

    [Command]
    void CmdEnemyShoot(string shootEnemyId, float damage)
    {
        Debug.Log(shootEnemyId + " has been shoot");
        GameManager.GetEnemy(shootEnemyId).RpcTakeDamage(damage);
    }

    [Command]
    void CmdPlayerShoot(string shootPlayerId, float damage)
    {
        Debug.Log(shootPlayerId + " has been shoot");
        GameManager.GetPlayer(shootPlayerId).RpcTakeDamage(damage);
    }
}
