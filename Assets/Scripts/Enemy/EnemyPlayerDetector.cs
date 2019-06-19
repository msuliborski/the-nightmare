using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPlayerDetector : MonoBehaviour
{

    private EnemyControllerServer _enemyControllerServer;
    // Start is called before the first frame update
    void Start()
    {
        if (GetComponentInParent<EnemyControllerServer>().enabled)
        {
            _enemyControllerServer = GetComponentInParent<EnemyControllerServer>();
        }
        else enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (enabled && !_enemyControllerServer.IsTriggerLocked && other.CompareTag("Player"))
        {
            _enemyControllerServer.PlayerDetected(other.GetComponentInParent<PlayerManager>().transform);
        }
    }


}
