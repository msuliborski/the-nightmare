
using UnityEngine;
using UnityEngine.Networking;

public class PlayerEquipment : NetworkBehaviour
{
    public AudioSource WeaponSound { get; set; }
    public PlayerWeapon Weapon { get; set; }
    
    [ClientRpc]
    public void RpcPlayerShooting(string connectionToClientId)
    {
        if (connectionToClient.connectionId.ToString() != connectionToClientId) WeaponSound.Play();
        Debug.Log(transform.name + " is shooting");
    }
}
