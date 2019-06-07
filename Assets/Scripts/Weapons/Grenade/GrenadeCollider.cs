﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GrenadeCollider : NetworkBehaviour
{
    public bool server = false;
    private float _damage;
    void Start()
    {
        _damage = transform.GetComponentInParent<Grenade>()._damage;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger");
        if (server)
        {
            Debug.Log("Is server");
            if (other.CompareTag("EnemyLegs") || other.CompareTag("EnemyHead") || other.CompareTag("EnemyBody"))
            {
                other.GetComponentInParent<EnemyControllerServer>().CmdTakeDamage(_damage);
            }
            else if (other.CompareTag("Player"))
            {
                other.GetComponentInParent<PlayerManager>().RpcTakeDamage(_damage);
            }
        }
    }
}