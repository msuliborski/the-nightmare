
using UnityEngine;
using UnityEngine.Networking;

public class PlayerEquipment : NetworkBehaviour
{
    public AudioSource WeaponSound { get; set; }
    public PlayerWeapon Weapon { get; set; }
    
    [ClientRpc]
    public void RpcPlayerShooting(string connectionId)
    {
        Debug.Log("1");
        if (connectionToClient == null)
        {
            Debug.Log("2");
            if (connectionId != "-1") WeaponSound.Play();
        }
        else if (connectionToClient.connectionId.ToString() != connectionId) { Debug.Log("3");  WeaponSound.Play(); }

        Debug.Log(transform.name + " is shooting");
    }

    [Command]
    public void CmdPlayerShooting(string connectionId)
    {
        
    }
}
