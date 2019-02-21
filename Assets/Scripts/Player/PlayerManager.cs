using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerManager : NetworkBehaviour
{
    [SerializeField] private int _maxHealth = 100;

    [SyncVar] private int _currentHealth;

    private void Start()
    {
        SetDefaults();
    }

    public void SetDefaults()
    {
        _currentHealth = _maxHealth;
    }

    public void TakeDamage(int damage)
    {
        _currentHealth -= damage;

        Debug.Log(transform.name + " nwo has " + _currentHealth + " health.");
    }
}
