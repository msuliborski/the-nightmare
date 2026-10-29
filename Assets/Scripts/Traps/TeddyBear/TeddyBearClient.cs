using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;

// Client-side presentation of a teddy bear. The server moves the bear (synced by NetworkTransformChild);
// clients only fall back to their own NavMeshAgent if that component is missing.
public class TeddyBearClient : NetworkBehaviour
{
    public NavMeshAgent Agent { get; set; }
    public Transform Dest { get; set; }
    public bool IsWalking { get; set; }
    private Animator _animator;
    private bool _simulateLocally;

    // This used to run in Awake, where isServer is always false, so the host simulated every bear twice.
    void Start()
    {
        if (isServer)
        {
            enabled = false;
            return;
        }
        IsWalking = true;
        Agent = transform.GetChild(1).GetComponent<NavMeshAgent>();
        _animator = GetComponentInChildren<Animator>();
        _simulateLocally = GetComponent<NetworkTransformChild>() == null;
        if (!_simulateLocally) Agent.enabled = false;
    }

    void Update()
    {
        if (_simulateLocally && Dest != null && Dest.gameObject.activeSelf && Agent.enabled) Agent.SetDestination(Dest.position);
    }

    public void SetWalking(bool isOn)
    {
        IsWalking = isOn;
        if (_simulateLocally && Agent != null) Agent.enabled = isOn;
    }

    public void SetAnim(string animName, bool isOn)
    {
        if (_animator != null) _animator.SetBool(animName, isOn);
    }
}
