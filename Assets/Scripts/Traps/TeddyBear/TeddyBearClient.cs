using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;

public class TeddyBearClient : NetworkBehaviour
{
    public NavMeshAgent Agent { get; set; }
    public Transform Dest { get; set; }
    public bool IsWalking { get; set; }
    private Animator _animator;
    
    void Awake()
    {
        if (isServer) enabled = false;
        else
        {
            IsWalking = true;
            Agent = transform.GetChild(1).GetComponent<NavMeshAgent>();
            _animator = GetComponentInChildren<Animator>();
        }
    }
    
    void Update()
    {
        if (Dest != null && Dest.gameObject.activeSelf && Agent.enabled) Agent.SetDestination(Dest.position);
    }
}
