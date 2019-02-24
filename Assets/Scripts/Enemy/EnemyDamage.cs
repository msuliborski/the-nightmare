using UnityEngine;
using UnityEngine.Networking;

public class EnemyDamage : NetworkBehaviour
{
    private PlayerManager _damageDest;
    private EnemyController _enemyController;
    [SerializeField] private float _damage = 2f;

    private void Start()
    {
        _enemyController = GetComponent<EnemyController>();    
    }
    
    // Update is called once per frame
    void Update()
    {
        if (_damageDest != null)
        {
            CmdPlayerTakeDamage();
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

    [Command]
    void CmdPlayerTakeDamage()
    {
        _damageDest.RpcTakeDamage(Time.deltaTime * _damage);
    }
    
}
