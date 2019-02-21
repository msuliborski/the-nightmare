
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
}
