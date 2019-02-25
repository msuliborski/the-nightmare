using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AI;

public class EnemyControllerClient : NetworkBehaviour
{

    public NavMeshAgent Agent { get; set; }

    public Transform Dest { get; set; }

    
    public bool IsWalking { get; set; }


    // Start is called before the first frame update
    void Start()
    {
        if (isServer) enabled = false;
        else
        {
            Agent = GetComponent<NavMeshAgent>();
            IsWalking = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Dest != null && IsWalking) Agent.SetDestination(Dest.position);
    }



}
