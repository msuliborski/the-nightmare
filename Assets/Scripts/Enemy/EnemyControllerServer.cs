using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class EnemyControllerServer : NetworkBehaviour
{

    private const string NO_DESTINATION = "";

    private const string ENEMY_ID_PREFIX = "Enemy ";

    public bool IsWalking { get; set; }
    private float _currentHealth;
    [SerializeField] private float _maxHealth = 50f;

    public NavMeshAgent Agent { get; set; }

    public Transform Dest { get; set; }
    private Animator _animator;

    public enum EnemyState { Walking, Screaming, Running, Fighting};
    private EnemyState _currentState = EnemyState.Walking;

    private void Start()
    {
        transform.name = ENEMY_ID_PREFIX + GameManager.EnemyIdCounter++;
        if (!GameManager.Enemies.ContainsKey(transform.name)) GameManager.Enemies.Add(transform.name, this);
        if (!isServer) enabled = false;
        else
        {
            StartCoroutine(SetClosestPlayerStart());
            IsWalking = true;
            _currentHealth = _maxHealth;
            _animator = GetComponent<Animator>();
        }
        Agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
       if (_currentState == EnemyState.Walking || _currentState == EnemyState.Running)
       {
            if (Dest != null && Dest.gameObject.activeSelf && IsWalking) Agent.SetDestination(Dest.position);
            else SetClosestPlayer();
       }
       else if (_currentState == EnemyState.Screaming)
       {
            if (!(_animator.GetCurrentAnimatorStateInfo(0).IsName("Scream") &&
            _animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f))
            {
                SetAnim("", false);
                _currentState = EnemyState.Running;
            }
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

        if (_currentHealth <= 0)
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
}
