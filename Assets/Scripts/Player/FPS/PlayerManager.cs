using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using Random = System.Random;

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
    [SerializeField] private GameObject _cameraReviving;
    private GameObject _camera;
    [SerializeField] private GameObject _aliveCollider;
    [SerializeField] private GameObject _reviveCollider;
    private GameObject boy;
    private GameObject girl;
    private NetworkAnimator _netAnim;
    


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
//            transform.GetChild(3).gameObject.SetActive(false);
            for (int i = 0; i < _disableOnDeath.Length; i++)
                _disableOnDeath[i].enabled = _wasEnabled[i];
            _cross.SetActive(true);
        }
       // else 
    }

    public void Setup()
    {
        _netAnim = GetComponent<NetworkAnimator>();
        boy = transform.GetChild(0).GetChild(0).gameObject;
        girl = transform.GetChild(0).GetChild(1).gameObject;
//        setModel();
        if (isLocalPlayer) SetLayerRecursively(transform.GetChild(0).gameObject, 12);
        _rigidbody = GetComponent<Rigidbody>();
        _wasEnabled = new bool[_disableOnDeath.Length];

        for (int i = 0; i < _wasEnabled.Length; i++) _wasEnabled[i] = _disableOnDeath[i].enabled;
        
        _camera = transform.GetChild(1).gameObject;
       _currentHealth = _maxHealth;
        _placementController = GetComponent<PlacementController>();
        if (isLocalPlayer) _cross = GameObject.Find("cross");
        //SetBuildingMode();
        SetActionMode();
    }

    void Update() {
        //changing weapon
        PlayerShoot t = transform.GetComponent<PlayerShoot>();
        if (!isLocalPlayer) {
            if (t.changeWeaponCooldown > 0) t.changeWeaponCooldown -= Time.deltaTime;
            if (t. changeWeaponCooldown <= 0 && Math.Abs(Input.GetAxis("Mouse ScrollWheel")) > 0.01 &&
                transform.GetComponent<PlayerEquipment>().getActiveWeapon().State == Weapon.WeaponState.idle && !t.IsBuildingOnFly) {
                t.changeWeaponCooldown = 2;
                if (transform.GetComponent<PlayerEquipment>().Weapon2 != null) {
                    if (transform.GetComponent<PlayerEquipment>().Weapon1.gameObject.activeSelf) {
                        StartCoroutine(t.HideWeapon(transform.GetComponent<PlayerEquipment>().Weapon1.gameObject,
                            transform.GetComponent<PlayerEquipment>().Weapon2.gameObject)); //show rifle
                    }
                    else {
                        StartCoroutine(t.HideWeapon(transform.GetComponent<PlayerEquipment>().Weapon2.gameObject,
                            transform.GetComponent<PlayerEquipment>().Weapon1.gameObject)); //show pistol
                    }
                }
            }
        }
    }

    private void setModel()
    {
        int rand = UnityEngine.Random.Range(0, 2);
        if (rand == 0)
        {
            boy.SetActive(true);
            girl.SetActive(false);
            _netAnim.animator = boy.GetComponent<Animator>();
            _playerAnimator = boy.GetComponent<Animator>();
        }
        else if (rand == 1)
        {
            boy.SetActive(false);
            girl.SetActive(true);
            _netAnim.animator = girl.GetComponent<Animator>();
            _playerAnimator = girl.GetComponent<Animator>();
        }

        for (int i = 0; i < 4; i++)
        {
            _netAnim.SetParameterAutoSend(i, true);
        }
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
            _disableOnDeath[i].enabled = _wasEnabled[i];

        
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
        _aliveCollider.SetActive(isOn);
        _reviveCollider.SetActive(!isOn);
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
        for (int i = 0; i < _disableOnDeath.Length; i++)
            _disableOnDeath[i].enabled = false;
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
        _rigidbody.isKinematic = true;

        CmdSwitchColliders(false);
        GameManager.DeactivatePlayer(transform.name);
        ChangeCamera(true);
        isRevived = true;
        
        transform.GetComponent<PlayerEquipment>().getActiveWeapon().transform.GetComponent<Animator>().SetBool("isHidden", false);
        transform.GetComponent<PlayerEquipment>().getActiveWeapon().transform.GetComponent<Animator>().SetBool("isSprinting", false);
        transform.GetComponent<PlayerEquipment>().getActiveWeapon().transform.GetComponent<Animator>().SetBool("isAiming", false);
        transform.GetComponent<PlayerEquipment>().getActiveWeapon().transform.GetComponent<Animator>().SetBool("isReloading", false);
    }
    

    private void ChangeCamera(bool isRevivng)
    {
        if (isLocalPlayer)
        {
            _camera.SetActive(!isRevivng);
            _cameraReviving.SetActive(isRevivng);
        }
    }
    
    public void Revive()
    {
        Debug.Log("reviving shit");
        _rigidbody.isKinematic = false;
        _rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        isRevived = false;
        ChangeCamera(false);
        _playerAnimator.SetBool("revive", true);
        //yield return new WaitForSeconds(GameManager.Instance.MatchSettings.RespawnTime);
        SetDefaults();
        Transform spawnPoint = NetworkManager.singleton.GetStartPosition();
//        transform.position = spawnPoint.position;
//        transform.rotation = spawnPoint.rotation;
        GameManager.ActivatePlayer(transform.name, this);
        

        if (transform.GetComponent<PlayerEquipment>().getActiveWeapon().Id == 0)
        {
            transform.GetComponent<PlayerEquipment>().getActiveWeapon().transform.localPosition = new Vector3(0.02f, 0.03f, -0.22f);
            transform.GetComponent<PlayerEquipment>().getActiveWeapon().transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        }
        else
        {
            transform.GetComponent<PlayerEquipment>().getActiveWeapon().transform.localPosition = new Vector3(0.08f, -0.02f, -0.17f);
            transform.GetComponent<PlayerEquipment>().getActiveWeapon().transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }

    public override void OnDeserialize(NetworkReader reader, bool initialState)
    {
        base.OnDeserialize(reader, initialState);
    }
}
