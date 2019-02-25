using UnityEngine;
using UnityEngine.Networking;

public class EnemyDamage : NetworkBehaviour
{
    private PlayerManager _damageDest;
    private EnemyControllerServer _enemyController;
    [SerializeField] private float _damage = 2f;

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
        if (other.CompareTag("Player") && enabled)
        {
            _damageDest = other.GetComponentInParent<PlayerManager>();
            _enemyController.Agent.enabled = false;
            _enemyController.IsWalking = false;
            RpcTurnOnWalking(false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && enabled)
        {
            _damageDest = null;
            _enemyController.Agent.enabled = true;
            _enemyController.IsWalking = true;
            //RpcTurnOnWalking(true);
        }
    }

    [ClientRpc]
    void RpcTurnOnWalking(bool isOn)
    {
        if (!isServer)
        {
            Debug.Log("setting!");
            EnemyControllerClient enemyControllerClient = GetComponent<EnemyControllerClient>();
            enemyControllerClient.Agent.enabled = isOn;
            enemyControllerClient.IsWalking = isOn;

        }
    }
}
