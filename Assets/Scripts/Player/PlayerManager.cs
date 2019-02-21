using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerManager : NetworkBehaviour
{

    [SyncVar] private bool _isDead = false;
    public bool IsDead { get { return _isDead; }  protected set { _isDead = value; } }

    [SerializeField] private int _maxHealth = 100;

    [SyncVar] private int _currentHealth;

    [SerializeField] private Behaviour[] _disableOnDeath;
    private bool[] _wasEnabled;

    public void Setup()
    {
        _wasEnabled = new bool[_disableOnDeath.Length];

        for (int i = 0; i < _wasEnabled.Length; i++) _wasEnabled[i] = _disableOnDeath[i].enabled;
        
        SetDefaults();
    }

    public void SetDefaults()
    {
        _isDead = false;

        _currentHealth = _maxHealth;

        for (int i = 0; i < _disableOnDeath.Length; i++)
            _disableOnDeath[i].enabled = _wasEnabled[i];

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = true; 
    }

    [ClientRpc]
    public void RpcTakeDamage(int damage)
    {
        if (_isDead) return;

        _currentHealth -= damage;

        Debug.Log(transform.name + " nwo has " + _currentHealth + " health.");

        if (_currentHealth <= 0) Die();
    }

    private void Die()
    {
        _isDead = true;

        foreach (Behaviour behaviour in _disableOnDeath)
            behaviour.enabled = false;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        StartCoroutine(Respawn());
    }


    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(GameManager.Instance.MatchSettings.RespawnTime);
        SetDefaults();
        Transform spawnPoint = NetworkManager.singleton.GetStartPosition();
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;
    }
}
