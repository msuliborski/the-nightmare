using System.Collections;
using UnityEngine;

using UnityEngine.Networking;

public class TrapsHandler : NetworkBehaviour
{
    private Snares _snares;
    private EnemyControllerServer _enemyController;


    private void Start()
    {
        _enemyController = GetComponentInParent<EnemyControllerServer>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Snares") && _enemyController.enabled)
        {

            Debug.Log("Snares kurwa");
            _enemyController.TurnOnWalking(false);
            _enemyController.CurrentState = EnemyControllerServer.EnemyState.Blocked;
            _enemyController.SetAnim("blocked", true);
            _snares = other.GetComponent<Snares>();
            StartCoroutine(Freeze());
            _snares.EnemiesCounter++;
            if (_snares.EnemiesCounter == _snares.EnemiesToDestroy)
            {
                Debug.Log("aaaa");
                if (_snares == null) Debug.Log("eeeee");
                if (_snares.InitialPosAndTag == null) Debug.Log("iiiiiii");
                if (_snares.InitialPosAndTag.pos == null) Debug.Log("iiiiiiieeee");
                if (_snares.InitialPosAndTag.tag == null) Debug.Log("iiiiiiiffffffffff");
                RpcUnlockSnaresPos(_snares.InitialPosAndTag.pos, _snares.InitialPosAndTag.tag);
                //Rpcg();
                //NetworkServer.Destroy(_snares.gameObject);
            }
        }
    }

    [ClientRpc]
    public void Rpcg() { Debug.Log("ejjj"); }
    [ClientRpc]
    void RpcUnlockSnaresPos(Vector2 pos, string tag)
    {
        Debug.Log("oooo");
        if (GameManager.Instance.BuildingPoints[new GameManager.PosAndTag(pos, tag)] == null) Debug.Log("hhhhh");
        GameManager.Instance.BuildingPoints[new GameManager.PosAndTag(pos, tag)].Buildable = true;
        Destroy(gameObject);
    }
    
    IEnumerator Freeze()
    {
        yield return new WaitForSeconds(_snares.freezeTime);
        _enemyController._damageDest = null;
        _enemyController.TurnOnWalking(true);
        _enemyController.CurrentState = _enemyController.PreviousState;
        switch (_enemyController.CurrentState)
        {
            case EnemyControllerServer.EnemyState.Running:
                _enemyController.SetAnim("running", true);
                _enemyController.SetAnim("blocked", false);
                break;
            case EnemyControllerServer.EnemyState.Walking:
                _enemyController.SetAnim("blocked", false);
                break;
        }
    }
}
