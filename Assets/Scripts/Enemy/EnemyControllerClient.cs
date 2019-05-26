using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AI;

public class EnemyControllerClient : NetworkBehaviour
{

    public NavMeshAgent Agent { get; set; }

    public Transform Dest { get; set; }

    private AudioSource _source;
    public bool IsWalking { get; set; }
    private Animator _animator;

    // Start is called before the first frame update
    void Start()
    {
        if (isServer) enabled = false;
        else
        {
            _source = GetComponent<AudioSource>();
            Agent = GetComponent<NavMeshAgent>();
            IsWalking = true;
            _animator = GetComponentInChildren<Animator>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Dest != null && Dest.gameObject.activeSelf && IsWalking) Agent.SetDestination(Dest.position);
    }

    public void SetAnim(string animName, bool isOn)
    {
        _animator.SetBool(animName, isOn);
    }

    public void Scream()
    {
        _source.PlayOneShot(_source.clip);

    }

    public void SetAgentSpeed(float speed)
    {
        Agent.speed = speed;
    }

}
