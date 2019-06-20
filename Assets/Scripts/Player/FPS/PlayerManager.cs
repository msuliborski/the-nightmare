using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerManager : NetworkBehaviour
{

    [SyncVar] private bool _isDead = false;
    public bool IsDead { get { return _isDead; }  protected set { _isDead = value; } }

    public float _maxHealth = 100;

    [SyncVar] public float _currentHealth;

    [SerializeField] private Behaviour[] _disableOnDeath;
    private bool[] _wasEnabled;

    private PlacementController _placementController;
    private GameObject _cross;
    private Rigidbody _rigidbody;
    private Animator _playerAnimator;
    private static bool isRevived = false;


    public void SetBuildingMode()
    {
        _rigidbody.useGravity = false;
        for (int i = 0; i < 3; i++) transform.GetChild(i).gameObject.SetActive(false);
        if (isLocalPlayer)
        {
            foreach (GameObject floor in GameManager.Instance.FloorsToDisable)
            {
                floor.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                floor.GetComponent<MeshCollider>().enabled = false;
            }
            transform.GetChild(3).gameObject.SetActive(true);
            for (int i = 0; i < _disableOnDeath.Length; i++)
                _disableOnDeath[i].enabled = !_wasEnabled[i];
            _cross.SetActive(false);
        }
    }

    public void SetActionMode()
    {

        _rigidbody.useGravity = true;
        for (int i = 0; i < 3; i++) transform.GetChild(i).gameObject.SetActive(true);
        if (isLocalPlayer)
        {
            //for (int i = 1; i < 3; i++) transform.GetChild(i).gameObject.SetActive(true);
            foreach (GameObject floor in GameManager.Instance.FloorsToDisable)
            {
                floor.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                floor.GetComponent<MeshCollider>().enabled = true;
            }
            transform.GetChild(3).gameObject.SetActive(false);
            for (int i = 0; i < _disableOnDeath.Length; i++)
                _disableOnDeath[i].enabled = _wasEnabled[i];
            _cross.SetActive(true);
        }
       // else 
    }

    public void Setup()
    {
        if (isLocalPlayer) SetLayerRecursively(gameObject.transform.GetChild(0).gameObject, 12);
        _rigidbody = GetComponent<Rigidbody>();
        _wasEnabled = new bool[_disableOnDeath.Length];
        _playerAnimator = transform.GetChild(0).GetChild(0).GetComponent<Animator>();

        for (int i = 0; i < _wasEnabled.Length; i++) _wasEnabled[i] = _disableOnDeath[i].enabled;

       _currentHealth = _maxHealth;
        _placementController = GetComponent<PlacementController>();
        if (isLocalPlayer) _cross = GameObject.Find("cross");
        //SetBuildingMode();
        SetActionMode();
    }
    
    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            if (child == null) continue;
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    public void SetDefaults()
    {
        _isDead = false;

        _currentHealth = _maxHealth;

        for (int i = 0; i < _disableOnDeath.Length; i++)
            _disableOnDeath[i].enabled = true;//_wasEnabled[i];

        
        CmdSwitchColliders(true);
    }

    [Command]
    void CmdSwitchColliders(bool isOn)
    {
        RpcSwitchColliders(isOn);
    }

    [ClientRpc]
    void RpcSwitchColliders(bool isOn)
    {
        transform.GetChild(0).GetChild(0).GetChild(7).gameObject.SetActive(isOn);
        transform.GetChild(0).GetChild(0).GetChild(8).gameObject.SetActive(!isOn);
        Debug.Log("switching colliders");
    }
    
    [ClientRpc]
    public void RpcTakeDamage(float damage)
    {
        if (_isDead) return;

        _currentHealth -= damage;

        //Debug.Log(transform.name + " now has " + _currentHealth + " health.");

        if (_currentHealth <= 0) Die();
    }

    private void Die()
    {
        _playerAnimator.SetTrigger("die");
        _playerAnimator.SetBool("revive", false);
        _isDead = true;

        foreach (Behaviour behaviour in _disableOnDeath)
            behaviour.enabled = false;
        
        CmdSwitchColliders(false);
        GameManager.DeactivatePlayer(transform.name);
        ChangeCamera();
        isRevived = true;
    }
    

    private void ChangeCamera()
    {
        
    }

    [Command]
    public void CmdRevive()
    {
        RpcRevive();
    }
    
    [ClientRpc]
    public void RpcRevive()
    {
        Debug.Log("reviving shit");
        isRevived = false;
        _playerAnimator.SetBool("revive", true);
        //yield return new WaitForSeconds(GameManager.Instance.MatchSettings.RespawnTime);
        SetDefaults();
        Transform spawnPoint = NetworkManager.singleton.GetStartPosition();
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;
        GameManager.ActivatePlayer(transform.name, this);
    }

    public override void OnDeserialize(NetworkReader reader, bool initialState)
    {
        base.OnDeserialize(reader, initialState);
    }
}
