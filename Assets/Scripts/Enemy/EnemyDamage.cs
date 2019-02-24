using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyDamage : MonoBehaviour
{
    private PlayerManager _damageDest;
    private EnemyController _enemyController;

    private void Start()
    {
        _enemyController = GetComponentInParent<EnemyController>();    
    }

    // Update is called once per frame
    void Update()
    {
        if (_damageDest != null)
        {
            _damageDest.RpcTakeDamage(Time.deltaTime * 2f);
            if (_damageDest.IsDead)
            {
                _damageDest = null;
                _enemyController.Agent.enabled = true;
                _enemyController.IsWalking = true;
                _enemyController.SetClosestPlayer();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _damageDest = other.GetComponentInParent<PlayerManager>();
            _enemyController.Agent.enabled = false;
            _enemyController.IsWalking = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _damageDest = null;
            _enemyController.Agent.enabled = true;
            _enemyController.IsWalking = true;
        }
    }
}
