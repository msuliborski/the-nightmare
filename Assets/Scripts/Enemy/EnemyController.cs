using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class EnemyController : MonoBehaviour
{

    public bool IsWalking { get; set; }
    private float _currentHealth;
    [SerializeField] private float _maxHealth = 50f;

    public NavMeshAgent Agent { get; set; }

    public Transform Dest { get; set; }

    private void Start()
    {
        Agent = GetComponent<NavMeshAgent>();
        StartCoroutine(GetClosestPlayer());
        IsWalking = true;
        _currentHealth = _maxHealth;
    }

    private void Update()
    {
        if (Dest != null && IsWalking) Agent.SetDestination(Dest.position);
    }

    private IEnumerator GetClosestPlayer()
    {
        yield return new WaitForSeconds(1f);

        List<Transform> players = new List<Transform>();
        
        foreach (PlayerManager player in GameManager.Players.Values)
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
    }

     

}
