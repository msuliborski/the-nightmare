using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.NetworkSystem;

public class HostMigration : NetworkMigrationManager
{
    public override bool FindNewHost(out PeerInfoMessage newHostInfo, out bool youAreNewHost)
    {
        bool isHost = base.FindNewHost(out newHostInfo, out youAreNewHost);
        if (isHost)
        {

            Debug.Log("KURWAA!");
            foreach (EnemyControllerServer enemy in GameManager.Enemies.Values)
            {
                enemy.GetComponent<EnemyControllerClient>().enabled = false;
                enemy.GetComponent<EnemyDamage>().enabled = true;
                enemy.enabled = true;
            }
        }
        return isHost;
    }
    
    protected override void OnServerHostShutdown()
    {
        base.OnServerHostShutdown();
        foreach (EnemyControllerServer enemy in GameManager.Enemies.Values)
        {
            enemy.GetComponent<EnemyControllerClient>().enabled = true;
            enemy.GetComponent<EnemyDamage>().enabled = false;
            enemy.enabled = false;
        }
    }
    
    

}
