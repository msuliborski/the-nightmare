using System.Collections;
using UnityEngine;


public class TrapsHandler : MonoBehaviour
{
    private Snares _snares;
    [SerializeField] private EnemyControllerServer _enemyController;
    [SerializeField] private EnemyDamage _enemyDamage;

    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Snares"))
        {
            Debug.Log("Snares kurwa");
            _enemyController.Agent.enabled = false;
            _enemyController.IsWalking = false;
            _enemyDamage.RpcTurnOnWalking(false);
            _snares = other.GetComponent<Snares>();
            StartCoroutine(Freeze());
        }
    }
    
    IEnumerator Freeze()
    {
        yield return new WaitForSeconds(_snares.freezeTime);
        _enemyDamage._damageDest = null;
        _enemyController.Agent.enabled = true;
        _enemyController.IsWalking = true;
        _enemyDamage.RpcTurnOnWalking(true);
    }
}
