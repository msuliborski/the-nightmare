using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;

public class TeddyBearServer : NetworkBehaviour
{
    private const string NO_DESTINATION = "";
    public NavMeshAgent Agent { get; set; }
    private EnemyControllerServer _damageDest;
    private EnemyControllerServer _previousDamageDest;
    [SerializeField] private float _damage = 60f;
    public Transform Dest { get; set; }
    private Animator _animator;
    public string InitialPosAndTag { get; set; }
    private int _beatenEnemies = 0;
    [SerializeField] private int _beatenEnemiesToDeath = 5;
    SpriteRenderer _spriteRenderer;
    public SpriteRenderer SpriteRenderer { get { return _spriteRenderer; } }
    
    public enum BearState
    {
        Running,
        Fighting,
        Waiting,
    };

    private BearState _currentState = BearState.Waiting;
    public BearState CurrentState
    {
        get => _currentState;
        set
        {
            _currentState = value;
        }
    }

    private List<EnemyControllerServer> _enemies = new List<EnemyControllerServer>();
    private Transform _bearTransform;

    void Start()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        GameManager.SpriteRenderer.Add(_spriteRenderer);
        if (!isServer)
        {
            enabled = false;
        }
        else
        {
            _animator = GetComponentInChildren<Animator>();
            _bearTransform = transform.GetChild(1);
            Agent = transform.GetChild(1).GetComponent<NavMeshAgent>();
            StartCoroutine(WaitForEndOfEntryAnim());
        }
    }

    

   void Update()
    {
        foreach (var enemy in _enemies.ToList())
        {
            if (enemy == null || enemy._isDying) _enemies.Remove(enemy);
        }

        switch (_currentState)
        {
            case BearState.Fighting:
                
                if (_damageDest == null || _damageDest._isDying || !_damageDest.gameObject.activeSelf)
                {
                    _enemies.Remove(_damageDest);
                    _damageDest = null;
                    _currentState = BearState.Running;
                    TurnOnWalking(true);
                    SetClosestEnemy();
                    SetAnim("fighting", false);
                    if (_beatenEnemies >= _beatenEnemiesToDeath)
                    {
                        CmdDeath();
                    }
                }
                else _damageDest.CmdTakeDamage(Time.deltaTime * _damage);
                break;

            case BearState.Waiting:

                break;

            case BearState.Running:
                if (Dest != null && Dest.gameObject.activeSelf) Agent.SetDestination(Dest.position);
                else SetClosestEnemy();
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
        
    }

    public void DamageEntryDetected(Collider other)
    {
        if (enabled && _currentState == BearState.Running)
        {
            TurnOnWalking(false);
            _damageDest = other.GetComponentInParent<EnemyControllerServer>();
            if (_previousDamageDest != _damageDest) _beatenEnemies++;
            _currentState = BearState.Fighting;
            SetAnim("fighting", true);
        }
    }
    
    public void DamageExitDetected(Collider other)
    {
        if (enabled && _currentState == BearState.Fighting)
        {
            TurnOnWalking(true);
            _previousDamageDest = _damageDest;
            _damageDest = null;
            SetAnim("fighting", false);
            _currentState = BearState.Running;
        }
    }
    
    private IEnumerator WaitForEndOfEntryAnim()
    {
        TurnOnWalking(false);
        SetAnim("waiting", true);
        yield return new WaitForSeconds(2.97f);
        TurnOnWalking(true);
        _currentState = BearState.Running;
    }

    
    [Command]
    public void CmdDeath()
    {
        _currentState = BearState.Waiting;
        TurnOnWalking(false);
        SetAnim("death", true);
        StartCoroutine(Die());
    }

    private IEnumerator Die()
    {
        yield return new WaitForSeconds(3.9f);
        RpcDie(InitialPosAndTag);
     }

    [ClientRpc]
    public void RpcDie(string posAndTag)
    {
        GameManager.SpriteRenderer.Remove(_spriteRenderer);
        GameManager.Instance.BuildingPoints[posAndTag].Buildable = true;
        Destroy(gameObject);
    }
    
    public void SetClosestEnemy()
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
            SetAnim("waiting", false);
        }
        else
        {
            RpcSendDest(NO_DESTINATION);
            SetAnim("waiting", true);
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

    


    public void SetAnim(string animName, bool isOn)
    {
        _animator.SetBool(animName, isOn);
        RpcSetAnim(animName, isOn);
    }

    [ClientRpc]
    void RpcSetAnim(string animName, bool isOn)
    {
        if (!isServer)
        {
            TeddyBearClient teddyBearClient = GetComponent<TeddyBearClient>();
            teddyBearClient.SetAnim(animName, isOn);
        }
    }

}