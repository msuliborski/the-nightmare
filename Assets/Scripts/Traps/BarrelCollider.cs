using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrelCollider : MonoBehaviour
{
    public bool server = false;
    private float _damage;
    void Start()
    {
        _damage = transform.GetComponentInParent<Barrel>().damage;
    }

    private void OnTriggerEnter(Collider other)
    {
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
            else if (other.CompareTag("Barrel"))
            {
                other.GetComponent<Barrel>().Explode();
            }
        }
    }
}
