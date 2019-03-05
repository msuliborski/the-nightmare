using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;

public class EnemyDamage : NetworkBehaviour
{
    private PlayerManager _damageDest;
    private EnemyControllerServer _enemyController;
    [SerializeField] private float _damage = 2f;
    private Snares _snares;

    private void Start()
    {
        if (!isServer) enabled = false;
        else _enemyController = GetComponent<EnemyControllerServer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_damageDest != null)
        {
            _damageDest.RpcTakeDamage(Time.deltaTime * _damage);
            if (_damageDest.IsDead)
            {
                _damageDest = null;
                _enemyController.Agent.enabled = true;
                _enemyController.IsWalking = true;
                RpcTurnOnWalking(true);
                _enemyController.SetClosestPlayer();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (enabled)
        {
            if (other.CompareTag("Player"))
            {
                _damageDest = other.GetComponentInParent<PlayerManager>();
                _enemyController.Agent.enabled = false;
                _enemyController.IsWalking = false;
                RpcTurnOnWalking(false);
            }

            else if (other.CompareTag("Snares"))
            {
                _enemyController.Agent.enabled = false;
                _snares = other.GetComponent<Snares>();
                StartCoroutine(Freeze());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && enabled)
        {
            _damageDest = null;
            _enemyController.Agent.enabled = true;
            _enemyController.IsWalking = true;
            RpcTurnOnWalking(true);
        }
    }

    [ClientRpc]
    void RpcTurnOnWalking(bool isOn)
    {
        if (!isServer)
        {
            EnemyControllerClient enemyControllerClient = GetComponent<EnemyControllerClient>();
            enemyControllerClient.Agent.enabled = isOn;
            enemyControllerClient.IsWalking = isOn;
        }
    }

    IEnumerator Freeze()
    {
        yield return new WaitForSeconds(_snares.freezeTime);
        _enemyController.Agent.enabled = false;
    }
}
