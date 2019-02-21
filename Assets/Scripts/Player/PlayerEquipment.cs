
using UnityEngine;
using UnityEngine.Networking;

public class PlayerEquipment : NetworkBehaviour
{
    public AudioSource WeaponSound { get; set; }
    public PlayerWeapon Weapon { get; set; }
    
    [ClientRpc]
    public void RpcPlayerShooting(string connectionToServerId)
    {
        if (connectionToServer == null) Debug.Log("moooo");
        if (connectionToServer.connectionId.ToString() != connectionToServerId) WeaponSound.Play();
        Debug.Log(transform.name + " is shooting");
    }
}
