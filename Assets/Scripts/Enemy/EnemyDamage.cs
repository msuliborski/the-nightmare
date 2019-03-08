using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;

public class EnemyDamage : NetworkBehaviour
{
    public PlayerManager _damageDest;
    private EnemyControllerServer _enemyController;
    [SerializeField] private float _damage = 2f;
    

    private void Start()
    {
        _enemyController = GetComponent<EnemyControllerServer>();
        if (!isServer) enabled = false;
    }   

    // Update is called once per frame
    void Update()
    {
        if (_damageDest != null)
        {
            _damageDest.RpcTakeDamage(Time.deltaTime * _damage);
            if (_damageDest.IsDead || !_damageDest.gameObject.activeSelf)
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
                Debug.Log("collision with Player");
                _damageDest = other.GetComponentInParent<PlayerManager>();
                _enemyController.Agent.enabled = false;
                _enemyController.IsWalking = false;
                RpcTurnOnWalking(false);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && enabled)
        {
            _damageDest = null;
            _enemyController.Agent.enabled = true;
            _enemyController.IsWalking = true;
            RpcTurnOnWalking(true);
        }
    }

    [ClientRpc]
    public void RpcTurnOnWalking(bool isOn)
    {
        if (!isServer)
        {
            EnemyControllerClient enemyControllerClient = GetComponent<EnemyControllerClient>();
            enemyControllerClient.Agent.enabled = isOn;
            enemyControllerClient.IsWalking = isOn;
        }
    }

    
}
