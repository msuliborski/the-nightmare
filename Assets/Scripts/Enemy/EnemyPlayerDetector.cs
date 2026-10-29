using UnityEngine;
using UnityEngine.Networking;

public class EnemyPlayerDetector : MonoBehaviour
{
    private EnemyControllerServer _enemyControllerServer;

    void Start()
    {
        // Checking EnemyControllerServer.enabled raced with its own Start (which disables it on clients),
        // so clients could also "detect" players and try to send server RPCs.
        if (NetworkServer.active) _enemyControllerServer = GetComponentInParent<EnemyControllerServer>();
        else enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (enabled && _enemyControllerServer != null && !_enemyControllerServer.IsTriggerLocked && other.CompareTag("Player"))
        {
            _enemyControllerServer.PlayerDetected(other.GetComponentInParent<PlayerManager>().transform);
        }
    }
}
