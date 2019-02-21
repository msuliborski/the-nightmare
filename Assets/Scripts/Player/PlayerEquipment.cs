
using UnityEngine;
using UnityEngine.Networking;

public class PlayerEquipment : NetworkBehaviour
{
    public AudioSource WeaponSound { get; set; }
    public PlayerWeapon Weapon { get; set; }


    public void PlayerShooting()
    {
        WeaponSound.Play();
        Weapon.Flash.Play();
        Weapon.Smoke.Play();
    }
    

    [ClientRpc]
    public void RpcPlayerShooting()
    {
        if (!isLocalPlayer)
        {
            WeaponSound.Play();
            Weapon.Flash.Play();
            Weapon.Smoke.Play();
        }

    }

    public void DoHitEffect(Vector3 hitPoint, Vector3 normal)
    {
        GameObject hitEffect = Instantiate(Weapon.HitEffect, hitPoint, Quaternion.LookRotation(normal));
        Destroy(hitEffect, 2f);
    }


    [ClientRpc]
    public void RpcDoHitEffect(Vector3 hitPoint, Vector3 normal)
    {
        if (!isLocalPlayer)
        {
            GameObject hitEffect = Instantiate(Weapon.HitEffect, hitPoint, Quaternion.LookRotation(normal));
            Destroy(hitEffect, 2f);
        }
    }
}
