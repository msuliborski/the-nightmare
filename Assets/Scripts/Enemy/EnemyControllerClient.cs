using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AI;
using System.Collections.Generic;

// Client-side presentation of an enemy (animations, sounds). The server simulates the enemy and its
// NetworkTransform moves it on clients; clients used to run their own NavMeshAgent towards the same
// destination, which drifted so every player saw the monsters somewhere else.
public class EnemyControllerClient : NetworkBehaviour
{
    public NavMeshAgent Agent { get; set; }
    public Transform Dest { get; set; }

    private AudioSource _source;
    public bool IsWalking { get; set; }
    private Animator _animator;
    private bool _simulateLocally;
    public List<AudioClip> clips = new List<AudioClip>();

    void Start()
    {
        if (isServer) enabled = false;
        else
        {
            _source = GetComponent<AudioSource>();
            Agent = GetComponent<NavMeshAgent>();
            IsWalking = true;
            _animator = GetComponentInChildren<Animator>();
            _simulateLocally = GetComponent<NetworkTransform>() == null;
            if (!_simulateLocally && Agent != null) Agent.enabled = false;
        }
    }

    void Update()
    {
        if (_simulateLocally && Dest != null && Dest.gameObject.activeSelf && IsWalking) Agent.SetDestination(Dest.position);
    }

    public void SetWalking(bool isOn)
    {
        IsWalking = isOn;
        if (Agent != null && Agent.enabled && Agent.isOnNavMesh) Agent.isStopped = !isOn;
    }

    public void SetAnim(string animName, bool isOn)
    {
        if (_animator != null) _animator.SetBool(animName, isOn);
    }

    public void Scream(int random)
    {
        if (_source == null || random < 0 || random >= clips.Count) return;
        _source.clip = clips[random];
        _source.PlayOneShot(_source.clip);
    }

    public void SetAgentSpeed(float speed)
    {
        if (Agent != null) Agent.speed = speed;
    }
}
