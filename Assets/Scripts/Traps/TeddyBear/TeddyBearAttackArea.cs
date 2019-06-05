using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TeddyBearAttackArea : NetworkBehaviour
{
    void Start()
    {
        if (!isServer) enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("EnemyLegs"))
            transform.parent.GetComponent<TeddyBearServer>().EntryDetected(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("EnemyLegs"))
            transform.parent.GetComponent<TeddyBearServer>().ExitDetected(other);
    }
}
