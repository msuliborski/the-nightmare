
using UnityEngine;
using UnityEngine.Networking;

public class PlayerEquipment : NetworkBehaviour
{
    public AudioSource WeaponSound { get; set; }
    public PlayerWeapon Weapon { get; set; }
    
    [ClientRpc]
    public void RpcPlayerShooting(string connectionId)
    {
        if (connectionToClient == null)
        {
            if (connectionId != "-1") WeaponSound.Play();
        }
        else if (connectionToClient.connectionId.ToString() != connectionId) WeaponSound.Play();

        Debug.Log(transform.name + " is shooting");
    }

    [Command]
    public void CmdPlayerShooting(string connectionId)
    {
        
    }
}
