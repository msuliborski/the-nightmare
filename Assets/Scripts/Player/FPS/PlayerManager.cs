using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerManager : NetworkBehaviour
{

    [SyncVar] private bool _isDead = false;
    public bool IsDead { get { return _isDead; }  protected set { _isDead = value; } }

    [SerializeField] private float _maxHealth = 100;

    [SyncVar] private float _currentHealth;

    [SerializeField] private Behaviour[] _disableOnDeath;
    private bool[] _wasEnabled;

    private PlacementController _placementController;
    private GameObject _cross;

    public void SetBuildingMode()
    {
        for (int i = 0; i < 3; i++) transform.GetChild(i).gameObject.SetActive(false);
        if (isLocalPlayer)
        {
            transform.GetChild(3).gameObject.SetActive(true);
            if (_placementController.GridCanvas != null)
                _placementController.GridCanvas.gameObject.SetActive(true);
            for (int i = 0; i < _disableOnDeath.Length; i++)
                _disableOnDeath[i].enabled = !_wasEnabled[i];
            _cross.SetActive(false);
        }
    }

    public void SetActionMode()
    {
        

        for (int i = 0; i < 3; i++) transform.GetChild(i).gameObject.SetActive(true);
        if (isLocalPlayer)
        {
            transform.GetChild(3).gameObject.SetActive(false);
            for (int i = 0; i < _disableOnDeath.Length; i++)
                _disableOnDeath[i].enabled = _wasEnabled[i];
            if (_placementController.GridCanvas != null)
                _placementController.GridCanvas.gameObject.SetActive(false);
            _cross.SetActive(true);
        }
    }

    public void Setup()
    {
       

        _wasEnabled = new bool[_disableOnDeath.Length];

        for (int i = 0; i < _wasEnabled.Length; i++) _wasEnabled[i] = _disableOnDeath[i].enabled;

       _currentHealth = _maxHealth;
        _placementController = GetComponent<PlacementController>();
        if (isLocalPlayer) _cross = GameObject.Find("cross");
        SetBuildingMode();
        
        //SetActionMode();
    }

    public void SetDefaults()
    {
        _isDead = false;

        _currentHealth = _maxHealth;

        for (int i = 0; i < _disableOnDeath.Length; i++)
            _disableOnDeath[i].enabled = _wasEnabled[i];

        transform.GetChild(0).gameObject.SetActive(true);
        transform.GetChild(1).gameObject.SetActive(true);
        
    }

    [ClientRpc]
    public void RpcTakeDamage(float damage)
    {
        if (_isDead) return;

        _currentHealth -= damage;

        Debug.Log(transform.name + " now has " + _currentHealth + " health.");

        if (_currentHealth <= 0) Die();
    }

    private void Die()
    {
        _isDead = true;

        foreach (Behaviour behaviour in _disableOnDeath)
            behaviour.enabled = false;

        transform.GetChild(0).gameObject.SetActive(false);
        transform.GetChild(1).gameObject.SetActive(false);
        GameManager.DeactivatePlayer(transform.name);
        StartCoroutine(Respawn());
    }


    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(GameManager.Instance.MatchSettings.RespawnTime);
        SetDefaults();
        Transform spawnPoint = NetworkManager.singleton.GetStartPosition();
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;
        GameManager.ActivatePlayer(transform.name, this);
    }
}
