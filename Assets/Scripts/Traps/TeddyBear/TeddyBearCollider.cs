using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TeddyBearCollider : NetworkBehaviour
{
    void Start()
    {
        if (!isServer) enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("EnemyLegs") || other.CompareTag("EnemyHead") || other.CompareTag("EnemyBody"))
            transform.parent.GetComponent<TeddyBearServer>().DamageEntryDetected(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("EnemyLegs") || other.CompareTag("EnemyHead") || other.CompareTag("EnemyBody"))
            transform.parent.GetComponent<TeddyBearServer>().DamageExitDetected(other);
    }
}
