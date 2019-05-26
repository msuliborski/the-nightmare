using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class EnemyControllerServer : NetworkBehaviour
{

    private const string NO_DESTINATION = "";

    private const string ENEMY_ID_PREFIX = "Enemy ";

    
    private AudioSource _source;
    public bool IsWalking { get; set; }
    private float _currentHealth;
    [SerializeField] private float _maxHealth = 50f;
    private float _screamTimer = 1f;
    public NavMeshAgent Agent { get; set; }
    public PlayerManager _damageDest;
    private bool _isDying;
    [SerializeField] private float _damage = 2f;
    public Transform Dest { get; set; }
    private Animator _animator;
    private CaptureArea _area;
    public enum EnemyState { Walking, Screaming, Running, Fighting, Blocked};
    private EnemyState _currentState = EnemyState.Walking;
    public EnemyState PreviousState { get; set; }
    public EnemyState CurrentState { get => _currentState;
        set
        {
            if (_currentState == EnemyState.Walking || _currentState == EnemyState.Running)
                PreviousState = _currentState;
            _currentState = value;
        }
    }

  

    private bool _init = false;

    private void Start()
    {
        transform.name = ENEMY_ID_PREFIX + GameManager.EnemyIdCounter++;
        if (!GameManager.Enemies.ContainsKey(transform.name)) GameManager.Enemies.Add(transform.name, this);
        if (!isServer) enabled = false;
        else
        {
            _source = GetComponent<AudioSource>();
            StartCoroutine(SetClosestPlayerStart());
            IsWalking = true;
            _currentHealth = _maxHealth;
            _animator = GetComponentInChildren<Animator>();
            
        }
        Agent = GetComponent<NavMeshAgent>();
        Agent.speed = 2f;
    }

    private void Update()
    {
        switch(_currentState)
        {
            case EnemyState.Walking:
                if (Dest != null && Dest.gameObject.activeSelf) Agent.SetDestination(Dest.position);
                else SetClosestPlayer();
                _screamTimer -= Time.deltaTime;
                if (Agent.remainingDistance < 6f && _screamTimer <= 0f)
                {
                    _currentState = EnemyState.Screaming;
                    Scream();
                    TurnOnWalking(false);
                    SetAnim("screaming", true);
                    _screamTimer = 0f;
                }
                break;

            case EnemyState.Running:
                if (Dest != null && Dest.gameObject.activeSelf) Agent.SetDestination(Dest.position);
                else SetClosestPlayer();
                break;

            case EnemyState.Screaming:

                _screamTimer += Time.deltaTime;
                if (_screamTimer >= 2.2)
                {
                    SetAnim("screaming", false);
                    SetAnim("running", true);
                    _currentState = EnemyState.Running;
                    TurnOnWalking(true);
                    SetAgentSpeed(3.5f);
                }
                break;

            case EnemyState.Fighting:

                _damageDest.RpcTakeDamage(Time.deltaTime * _damage);
                if (_damageDest.IsDead || !_damageDest.gameObject.activeSelf)
                {
                    _damageDest = null;
                    TurnOnWalking(true);
                    SetClosestPlayer();
                    SetAnim("running", true);
                }
                break;

            case EnemyState.Blocked:

                break;
        }
    }

    private IEnumerator SetClosestPlayerStart()
    {
        yield return new WaitForSeconds(1f);

        List<Transform> players = new List<Transform>();
        
        foreach (PlayerManager player in GameManager.ActivePlayers.Values)
            players.Add(player.transform);
        
        Transform tMin = null;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = transform.position;
        foreach (Transform t in players)
        {
            float dist = Vector3.Distance(t.position, currentPos);
            if (dist < minDist)
            {
                tMin = t;
                minDist = dist;
            }
        }
        Dest =  tMin;
        if (Dest) RpcSendDest(Dest.name);
        else RpcSendDest(NO_DESTINATION);
    }

    public void SetClosestPlayer()
    {
        List<Transform> players = new List<Transform>();

        foreach (PlayerManager player in GameManager.ActivePlayers.Values)
            players.Add(player.transform);

        Transform tMin = null;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = transform.position;
        foreach (Transform t in players)
        {
            float dist = Vector3.Distance(t.position, currentPos);
            if (dist < minDist)
            {
                tMin = t;
                minDist = dist;
            }
        }
        Dest = tMin;
        if (Dest) RpcSendDest(Dest.name);
        else RpcSendDest(NO_DESTINATION);
    }


    public void TakeDamage(float damage)
    {
        _currentHealth -= damage;

        if (_currentHealth <= 0 && !_isDying)
        {
            _isDying = true;
            TurnOnWalking(false);
            SetAnim("die", true);
            StartCoroutine(Die());
            _currentState = EnemyState.Blocked;
        }
            
        
    }


    IEnumerator Die()
    {
        yield return new WaitForSeconds(3.2f);
        if (_area != null)
            _area.DecrementEnemies();
        RpcRemoveEnemy();
    }


    [ClientRpc]
    void RpcRemoveEnemy()
    {
        GameManager.Enemies.Remove(transform.name);
        Destroy(gameObject);
    }


    [ClientRpc]
    void RpcSendDest(string destId)
    {
        if (!isServer)
        {
            EnemyControllerClient enemyControllerClient = GetComponent<EnemyControllerClient>();
            PlayerManager player = GameManager.GetPlayer(destId);
            if (player) enemyControllerClient.Dest = player.transform;
            else enemyControllerClient.Dest = null;
        }
    }

    [ClientRpc]
    void RpcSendTransform(Vector3 position, Quaternion rotation)
    {
        if (!isServer)
        {
            transform.position = position;
            transform.rotation = rotation;
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
            EnemyControllerClient enemyControllerClient = GetComponent<EnemyControllerClient>();
            enemyControllerClient.SetAnim(animName, isOn);
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (enabled && CurrentState == EnemyState.Running)
        {
            if (other.CompareTag("Player"))
            {
                Debug.Log("collision with Player");
                _damageDest = other.GetComponentInParent<PlayerManager>();
                TurnOnWalking(false);
                CurrentState = EnemyState.Fighting;
                SetAnim("running", false);
            }
        }
        
        if (other.CompareTag("CaptureArea"))
        {
            _area = other.GetComponent<CaptureArea>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && enabled && CurrentState == EnemyState.Fighting)
        {
            _damageDest = null;
            TurnOnWalking(true);
            SetAnim("running", true);
            CurrentState = EnemyState.Running;
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
            EnemyControllerClient enemyControllerClient = GetComponent<EnemyControllerClient>();
            enemyControllerClient.Agent.enabled = isOn;
            enemyControllerClient.IsWalking = isOn;
        }
    }


    void Scream()
    {
        _source.PlayOneShot(_source.clip);

    }


    void SetAgentSpeed(float speed)
    {
        Agent.speed = speed;
        RpcSetAgentSpeed(speed);
    }


    [ClientRpc]
    void RpcSetAgentSpeed(float speed)
    {
        if (!isServer)
        {
            EnemyControllerClient enemyControllerClient = GetComponent<EnemyControllerClient>();
            enemyControllerClient.SetAgentSpeed(speed);
        }
    }


    [ClientRpc]
    void RpcScream()
    {
        if (!isServer)
        {
            EnemyControllerClient enemyControllerClient = GetComponent<EnemyControllerClient>();
            enemyControllerClient.Scream();
        }
    }

}
