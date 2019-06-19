using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class EnemyControllerServer : NetworkBehaviour
{

    private const string NO_DESTINATION = "";

    private const string ENEMY_ID_PREFIX = "Enemy ";

    private const float SCREAM_TIME = 2.2f;
    private const float DIST_TO_SCREAM = 6f;
    private AudioSource _source;
    public bool IsWalking { get; set; }
    [SyncVar] private float _currentHealth;
    [SerializeField] private float _maxHealth = 50f;
    private float _screamTimer = 1f;
    public NavMeshAgent Agent { get; set; }
    public PlayerManager _damageDest;
    public bool _isDying;
    [SerializeField] private float _damage = 2f;
    [SerializeField] private List<Collider> _colliders;
    public Transform Dest { get; set; }
    private Animator _animator;
    private CaptureArea _area;
    public enum EnemyState { Walking, Screaming, Running, Fighting, Blocked};
    private EnemyState _currentState = EnemyState.Walking;
    public EnemyState PreviousState { get; set; }
    public EnemyState CurrentState { get => _currentState;
        set
        {
            PreviousState = _currentState;
            _currentState = value;
        }
    }
    public List<AudioClip> clips = new List<AudioClip>();

    public Snares SnaresToDestroy { get; set; }

    private bool _init = false;
    public bool IsTriggerLocked { get; set; }

    public bool ShouldScream { get; set; }
    private void Start()
    {
        transform.name = ENEMY_ID_PREFIX + GameManager.EnemyIdCounter++;
        GameManager.Enemies.Add(transform.name, this);
        if (!isServer) enabled = false;
        else
        {
            int randInt = Random.RandomRange(0, 5);
            if (randInt == 0) ShouldScream = true;
            _source = GetComponent<AudioSource>();
            StartCoroutine(SetClosestCaptureAreaStart());
            IsWalking = true;
            _currentHealth = _maxHealth;
            _animator = GetComponentInChildren<Animator>();
            
        }
        Agent = GetComponent<NavMeshAgent>();
        //Obstacle = GetComponent<NavMeshObstacle>();
        Agent.speed = 2f;
    }

    private void Update()
    {
        switch(_currentState)
        {
            case EnemyState.Walking:
                if (Dest != null && Dest.gameObject.activeSelf)
                {
                    if (!Agent.SetDestination(Dest.position))
                    {
                        SetClosestCaptureArea();
                    }
                }
                else SetClosestCaptureArea();
                _screamTimer -= Time.deltaTime;
                if (Agent.remainingDistance < DIST_TO_SCREAM && _screamTimer <= 0f)
                {
                    if (ShouldScream)
                    {
                        _currentState = EnemyState.Screaming;
                        Scream();
                        TurnOnWalking(false);
                        SetAnim("screaming", true);
                        _screamTimer = 0f;
                    }
                    else
                    {
                        SetAnim("running", true);
                        _currentState = EnemyState.Running;
                        SetAgentSpeed(3.5f);
                    }
                }
                break;

            case EnemyState.Running:
                if (Dest != null && Dest.gameObject.activeSelf)
                {
                    if (!Agent.SetDestination(Dest.position))
                    {
                        SetClosestCaptureArea();
                    }
                }
                else SetClosestCaptureArea();
                break;

            case EnemyState.Screaming:

                _screamTimer += Time.deltaTime;
                if (_screamTimer >= SCREAM_TIME)
                {
                    SetAnim("screaming", false);
                    SetAnim("running", true);
                    _currentState = EnemyState.Running;
                    TurnOnWalking(true);
                    SetAgentSpeed(3.5f);
                }
                break;

            case EnemyState.Fighting:

                if (_damageDest == null || _damageDest.IsDead || !_damageDest.gameObject.activeSelf)
                {
                    _damageDest = null;
                    TurnOnWalking(true);
                    SetClosestCaptureArea();
                    SetAnim("running", true);
                    _currentState = EnemyState.Walking;
                }
                else _damageDest.RpcTakeDamage(Time.deltaTime * _damage);
                

                break;

            case EnemyState.Blocked:

                break;
        }
    }


    public void PlayerDetected(Transform playerTransform)
    {
        Dest = playerTransform;
        IsTriggerLocked = true;
    }

    private IEnumerator SetClosestCaptureAreaStart()
    {
        yield return new WaitForSeconds(1f);

        List<Transform> captureAreas = new List<Transform>();
        IsTriggerLocked = false;
        foreach (CaptureArea captureArea in GameManager.Instance.CurrentCaptureAreas)
        {
            captureAreas.Add(captureArea.transform);
            Debug.Log(captureArea.name);
        }

        Transform tMin = null;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = transform.position;
        foreach (Transform t in captureAreas)
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



    /*private IEnumerator SetClosestPlayerStart()
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
    }*/

    public void SetClosestCaptureArea()
    {
        List<Transform> captureAreas = new List<Transform>();
        IsTriggerLocked = false;
        foreach (CaptureArea captureArea in GameManager.Instance.CurrentCaptureAreas)
        { captureAreas.Add(captureArea.transform);
            Debug.Log(captureArea.name); }

        Transform tMin = null;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = transform.position;
        foreach (Transform t in captureAreas)
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

    [Command]
    public void CmdTakeDamage(float damage)
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

    public
    IEnumerator Die()
    {
        RpcRemoveEnemy();
        if (_area != null)
            _area.CmdDecrementEnemies();
        yield return new WaitForSeconds(3.2f);
        RpcDie();
    }


    [ClientRpc]
    void RpcRemoveEnemy()
    {
        Transform body = transform.GetChild(0).GetChild(0);
        foreach (Collider collider in _colliders)
        {
            collider.gameObject.SetActive(false);
        }
        GameManager.Enemies.Remove(transform.name);
    }

    

    [ClientRpc]
    void RpcDie()
    {
        transform.GetChild(0).GetChild(0).GetChild(1).gameObject.SetActive(false); // turning off eyes
        GetComponentInChildren<DissolveEnemyBody>().Enabled = true;
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
        if (enabled)
        {
            if (other.CompareTag("Player") && CurrentState == EnemyState.Running)
            {
                Debug.Log("collision with Player");
                _damageDest = other.GetComponentInParent<PlayerManager>();
                TurnOnWalking(false);
                CurrentState = EnemyState.Fighting;
                SetAnim("running", false);
            }
            else if (other.CompareTag("CaptureArea"))
            {
                _area = other.GetComponent<CaptureArea>();
            }
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
        Agent.isStopped = !isOn;
        //Agent.enabled = isOn;
        //Obstacle.enabled = !isOn;
        RpcTurnOnWalking(isOn);
    }


    [ClientRpc]
    public void RpcTurnOnWalking(bool isOn)
    {
        if (!isServer)
        {
            EnemyControllerClient enemyControllerClient = GetComponent<EnemyControllerClient>();
            //enemyControllerClient.Agent.enabled = isOn;
            enemyControllerClient.Agent.isStopped = !isOn;
            //enemyControllerClient.Obstacle.enabled = !isOn;
            enemyControllerClient.IsWalking = isOn;
        }
    }


    void Scream()
    {
        int random = Random.Range(0, clips.Count);
        _source.clip = clips[random];
        _source.PlayOneShot(_source.clip);
        RpcScream(random);

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
    void RpcScream(int random)
    {
        if (!isServer)
        {
            EnemyControllerClient enemyControllerClient = GetComponent<EnemyControllerClient>();
            enemyControllerClient.Scream(random);
        }
    }

}
