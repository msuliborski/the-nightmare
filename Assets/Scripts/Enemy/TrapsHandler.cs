using System.Collections;
using UnityEngine;

using UnityEngine.Networking;

public class TrapsHandler : MonoBehaviour
{
    private Snares _snares;
    private EnemyControllerServer _enemyController;


    private void Start()
    {
        _enemyController = GetComponentInParent<EnemyControllerServer>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Snares") && _enemyController.enabled && _enemyController.CurrentState != EnemyControllerServer.EnemyState.Blocked)
        {
            _enemyController.TurnOnWalking(false);
            _enemyController.CurrentState = EnemyControllerServer.EnemyState.Blocked;
            _enemyController.SetAnim("blocked", true);
            _snares = other.GetComponent<Snares>();
            StartCoroutine(Freeze());
            _snares.RegisterCaughtEnemy();
        }
    }

    
    
    IEnumerator Freeze()
    {
        yield return new WaitForSeconds(_snares.freezeTime);
        if (_enemyController._isDying) yield break; // don't revive a dying enemy's walk/run animation
        _enemyController._damageDest = null;
        _enemyController.CurrentState = _enemyController.PreviousState;
        switch (_enemyController.CurrentState)
        {
            case EnemyControllerServer.EnemyState.Running:
                _enemyController.SetAnim("running", true);
                _enemyController.SetAnim("blocked", false);
                _enemyController.TurnOnWalking(true);

                break;
            case EnemyControllerServer.EnemyState.Walking:
                _enemyController.SetAnim("blocked", false);
                _enemyController.TurnOnWalking(true);
                break;
        }
    }
}
