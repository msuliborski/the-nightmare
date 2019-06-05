using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;

public class TeddyBearServer : NetworkBehaviour
{
    [SerializeField] private float _lifespan;
    private const string NO_DESTINATION = "";
    public NavMeshAgent Agent { get; set; }
    private bool _isDying;
    private EnemyControllerServer _damageDest;
    [SerializeField] private float _damage = 60f;
    public Transform Dest { get; set; }
    private Animator _animator;
    
    public enum BearState
    {
        Running,
        Fighting,
        Waiting,
        Dying
    };

    private BearState _currentState = BearState.Dying;
    public BearState PreviousState { get; set; }

    public BearState CurrentState
    {
        get => _currentState;
        set
        {
            if (_currentState == BearState.Running)
                PreviousState = _currentState;
            _currentState = value;
        }
    }

    private List<EnemyControllerServer> _enemies = new List<EnemyControllerServer>();
    private Transform _bearTransform;

    void Start()
    {
        if (!isServer)
        {
            enabled = false;
            Debug.Log("bear client");
        }
        else
        {
            Debug.Log("bear server");
            _animator = GetComponentInChildren<Animator>();
            //StartCoroutine(SetClosestPlayerStart());
            _bearTransform = transform.GetChild(1);
            Agent = transform.GetChild(1).GetComponent<NavMeshAgent>();
            _isDying = true;
            StartCoroutine(SetClosestPlayerStart());
            StartCoroutine(Decay());
        }
    }

    

   void Update()
    {
        
        foreach (var enemy in _enemies.ToList())
        {
            if (enemy._isDying) _enemies.Remove(enemy);
        }
        if (_isDying)
        {
            _currentState = BearState.Dying;
        }
        else if (_enemies.Count == 0)
        {
            _currentState = BearState.Waiting;
            _animator.SetBool("waiting", true);
        }
        else if(_damageDest == null)
        {
            _currentState = BearState.Running;
            _animator.SetBool("waiting", false);
        }
        switch (_currentState)
        {
            case BearState.Fighting:
                Debug.Log("fighting");
                _damageDest.CmdTakeDamage(Time.deltaTime * _damage);
                if (_damageDest._isDying || !_damageDest.gameObject.activeSelf)
                {
                    _enemies.Remove(_damageDest);
                    _damageDest = null;
                    _currentState = BearState.Running;
                    TurnOnWalking(true);
                    SetClosestPlayer();
                    _animator.SetBool("fighting", false);
                }
                break;

            case BearState.Waiting:
                SetClosestPlayer();
                break;
                
            case BearState.Running:
                //TurnOnWalking(true);
                Debug.Log("running");
                if (Dest != null && Dest.gameObject.activeSelf) Agent.SetDestination(Dest.position);
                else SetClosestPlayer();
                break;
            
            case BearState.Dying:
                break;
        }
        
    }

    public void EntryDetected(Collider other)
    {
        _enemies.Add(other.GetComponentInParent<EnemyControllerServer>());
        
    }
    
    public void ExitDetected(Collider other)
    {
        _enemies.Remove(other.GetComponentInParent<EnemyControllerServer>());
        if (_enemies.Count == 0)
        {
            _currentState = BearState.Waiting;
        }
    }

    public void DamageEntryDetected(Collider other)
    {
        if (_currentState == BearState.Running)
        {
            TurnOnWalking(false);
            _damageDest = other.GetComponentInParent<EnemyControllerServer>();
            _currentState = BearState.Fighting;
            _animator.SetBool("fighting", true);
        }
    }
    
    public void DamageExitDetected(Collider other)
    {
        if (enabled && _currentState == BearState.Fighting)
        {
            TurnOnWalking(true);
            _damageDest = null;
            _animator.SetBool("fighting", false);
            _currentState = BearState.Running;
        }
        if (_enemies.Count == 0 && _damageDest == null)
        {
            _currentState = BearState.Waiting;
        }
    }
    
    private IEnumerator SetClosestPlayerStart()
    {
        TurnOnWalking(false);
        yield return new WaitForSeconds(2.97f);

        _isDying = false;
        TurnOnWalking(true);

        Transform tMin = null;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = _bearTransform.position;
        foreach (EnemyControllerServer t in _enemies)
        {
            float dist = Vector3.Distance(t.transform.position, currentPos);
            if (dist < minDist)
            {
                tMin = t.transform;
                minDist = dist;
            }
        }

        Dest = tMin;

        if (Dest)
        {
            RpcSendDest(Dest.name);
            _currentState = BearState.Running;
            
        }
        else
        {
            RpcSendDest(NO_DESTINATION);
            _currentState = BearState.Waiting;

            
        }
    }

    private IEnumerator Decay()
    {
        yield return new WaitForSeconds(_lifespan);
        CmdDeath();
    }

    [Command]
    public void CmdDeath()
    {
        _isDying = true;
        TurnOnWalking(false);
        _animator.SetBool("death", true);
        StartCoroutine(Die());
    }

    private IEnumerator Die()
    {
        yield return new WaitForSeconds(3.9f);
        RpcDie();
    }

    [ClientRpc]
    public void RpcDie()
    {
        Destroy(gameObject);
    }
    
    public void SetClosestPlayer()
    {
        Transform tMin = null;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = _bearTransform.position;
        foreach (EnemyControllerServer t in _enemies)
        {
            float dist = Vector3.Distance(t.transform.position, currentPos);
            if (dist < minDist)
            {
                tMin = t.transform;
                minDist = dist;
            }
        }
        
        Dest = tMin;

        if (Dest)
        {
            RpcSendDest(Dest.name);
            _currentState = BearState.Running;
            
        }
        else
        {
            RpcSendDest(NO_DESTINATION);
            _currentState = BearState.Waiting;
            
        }
    }

    [ClientRpc]
    void RpcSendDest(string destId)
    {
        if (!isServer)
        {
            TeddyBearClient teddyClient = GetComponent<TeddyBearClient>();
            EnemyControllerServer enemy = GameManager.GetEnemy(destId);
            if (enemy) teddyClient.Dest = enemy.transform;
            else teddyClient.Dest = null;
        }
    }
    
    public void TurnOnWalking(bool isOn)
    {
        Agent.enabled = isOn;
        RpcTurnOnWalking(isOn);
    }


    [ClientRpc]
    public void RpcTurnOnWalking(bool isOn)
    {
        if (!isServer)
        {
            TeddyBearClient teddyClient = GetComponent<TeddyBearClient>();
            teddyClient.Agent.enabled = isOn;
            teddyClient.IsWalking = isOn;
        }
    }
}