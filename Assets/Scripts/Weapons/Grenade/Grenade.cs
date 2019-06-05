using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Grenade : NetworkBehaviour
{
    [SerializeField] private float _secsToExplosion;
    [SerializeField] private float _decayTime;
    public float _damage;
    void Start()
    {
        StartCoroutine(Explode());
        if (isServer) transform.GetChild(0).GetComponent<GrenadeCollider>().server = true;
    }

    private IEnumerator Explode()
    {
        yield return new WaitForSeconds(_secsToExplosion);
        transform.GetChild(0).gameObject.SetActive(true);
        StartCoroutine(Decay());
    }

    private IEnumerator Decay()
    {
        yield return new WaitForSeconds(_decayTime);
        Destroy(gameObject);
    }
}
