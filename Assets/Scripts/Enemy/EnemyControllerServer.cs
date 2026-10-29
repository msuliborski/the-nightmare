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
    public enum EnemyState { Walking, Screaming, Running, Fighting, Blocked, InZone};
    private EnemyState _currentState = EnemyState.Walking;
    public EnemyState PreviousState { get; set; }
    public EnemyState CurrentState { get => _currentState;
        set
        {
            if (CurrentState == EnemyState.Running || CurrentState == EnemyState.Walking)
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
        // Hits and teddy-bear targets refer to enemies by name, so the name must be identical on every
        // peer: use the network id instead of a local spawn counter (which differed per client).
        transform.name = ENEMY_ID_PREFIX + netId;
        GameManager.Enemies[transform.name] = this;
        if (!isServer) enabled = false;
        else
        {
            int randInt = Random.Range(0, 5);
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
                else _damageDest.TakeDamage(Time.deltaTime * _damage);
                

                break;

            case EnemyState.Blocked:

                break;

            case EnemyState.InZone:

                break;
        }
    }


    public void PlayerDetected(Transform playerTransform)
    {
        Dest = playerTransform;
        
        RpcSendDest(playerTransform.name, false);
        IsTriggerLocked = true;
    }

    private IEnumerator SetClosestCaptureAreaStart()
    {
        yield return new WaitForSeconds(1f);

        List<Transform> captureAreas = new List<Transform>();
        IsTriggerLocked = false;
        foreach (CaptureArea captureArea in GameManager.Instance.CurrentCaptureAreas.Values)
        {
            captureAreas.Add(captureArea.transform);
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
        if (Dest)
        {
            string areaId = Dest.position.x.ToString() + "_" + Dest.position.y.ToString() + "_" + Dest.position.z.ToString();
            RpcSendDest(areaId, true);
        }
        else RpcSendDest(NO_DESTINATION, true);
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
        foreach (CaptureArea captureArea in GameManager.Instance.CurrentCaptureAreas.Values)
        {
            if (captureArea.EnemyNum < 7)
            {
                captureAreas.Add(captureArea.transform);
            }
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
        if (Dest)
        {
            string areaId = Dest.position.x.ToString() + "_" + Dest.position.y.ToString() + "_"  + Dest.position.z.ToString();
            RpcSendDest(areaId, true);
        }
        else RpcSendDest(NO_DESTINATION, true);
    }

    // Called by server-side code only (player hits arrive through PlayerShoot's Command). This was a
    // [Command] invoked on the server, which UNET only tolerated because the game always ran as host.
    [Server]
    public void TakeDamage(float damage)
    {
        if (_isDying) return;
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
        yield return new WaitForSeconds(3.2f);
        RpcDie();
    }


    [ClientRpc]
    void RpcRemoveEnemy()
    {
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
    void RpcSendDest(string destId, bool isArea)
    {
        if (!isServer)
        {
            if (isArea)
            {
                EnemyControllerClient enemyControllerClient = GetComponent<EnemyControllerClient>();
                CaptureArea captureArea;
                GameManager.Instance.CurrentCaptureAreas.TryGetValue(destId, out captureArea);
                enemyControllerClient.Dest = captureArea != null ? captureArea.transform : null;
            }
            else
            {
                EnemyControllerClient enemyControllerClient = GetComponent<EnemyControllerClient>();
                PlayerManager player = GameManager.GetPlayer(destId);
                if (player) enemyControllerClient.Dest = player.transform;
                else { enemyControllerClient.Dest = null; }
            }
        }
    }

    public void SetAnim(string animName, bool isOn)
    {
        _animator.SetBool(animName, isOn);
        RpcSetAnim(animName, isOn);
    }

    private void OnDestroy()
    {
        EnemyControllerServer registered;
        if (GameManager.Enemies.TryGetValue(transform.name, out registered) && registered == this)
            GameManager.Enemies.Remove(transform.name);
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
                _damageDest = other.GetComponentInParent<PlayerManager>();
                TurnOnWalking(false);
                CurrentState = EnemyState.Fighting;
                SetAnim("running", false);
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
            enemyControllerClient.SetWalking(isOn);
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
