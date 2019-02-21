
using UnityEngine;
using UnityEngine.Networking;

public class PlayerEquipment : NetworkBehaviour
{
    public AudioSource WeaponSound { get; set; }
    public PlayerWeapon Weapon { get; set; }
    
    [ClientRpc]
    public void RpcPlayerShooting(string connectionId)
    {
        if (!isServer)
        {
            if (connectionToServer.connectionId.ToString() != connectionId) WeaponSound.Play();
        }
        else if (connectionToClient.connectionId.ToString() != connectionId) WeaponSound.Play();

        Debug.Log(transform.name + " is shooting");
    }
}
