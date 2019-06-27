using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Collections;

[RequireComponent(typeof(PlayerManager))]
public class PlayerSetup : NetworkBehaviour {
    [SerializeField] private Behaviour[] _toDisable;
    [SerializeField] private Camera _buildingCamera;
    [SerializeField] private Camera _actionCamera;
    public Camera ActionCamera { get { return _actionCamera; } }
    private Camera _sceneCamera;
    [SerializeField] private GameObject _weaponObjectPrefab;
    private PlayerEquipment _equipment;
    private BulletsHUD _bulletshud;
    private HealthBar _healthBar;
    private ClipsManager _clipsManager;
    
    
    
    

    // Start is called before the first frame update
    void Start() {
        if (!isLocalPlayer) {
            EquipWeaponNotLocal();
            DisableComponents();
            AssignRemoteLayer();
            GetComponentInChildren<Arrow>().gameObject.SetActive(false);
        }
        else {
            Application.targetFrameRate = 50;
            EquipWeapon();
           
            _bulletshud = GameObject.Find("bulletsNumber").GetComponent<BulletsHUD>();
            _healthBar = GameObject.Find("HP").GetComponent<HealthBar>();
            _clipsManager = GameObject.Find("Clips").GetComponent<ClipsManager>();
            GameManager.LocalPlayer = GetComponent<PlayerManager>();
            //if (GameManager.Instance.CurrentMachState == GameManager.MatchState.None)
            //{
                _sceneCamera = GameObject.Find("SceneCamera").GetComponent<Camera>();
                if (_sceneCamera != null)
                    _sceneCamera.gameObject.SetActive(false);
                GameManager.Instance.CurrentMachState = GameManager.MatchState.Lobby;
                //GameManager.IsListeningForReady = true;
            //}
            //else
            //{
            //    GameManager.Instance.CurrentMachState = GameManager.MatchState.Room1Prepare;
            //}
            
            _bulletshud.Equipment = GetComponent<PlayerEquipment>();
            _clipsManager.player = GetComponent<PlayerEquipment>();
            _healthBar.player = GetComponent<PlayerManager>();
            _bulletshud.playerEnabled = true;
            _healthBar.playerEnabled = true;
            _clipsManager.playerEnabled = true;
            GameManager.Instance.SetCameraForBillboards(_actionCamera);
            
            GameObject g = GameObject.Find("LobbySpawn");
            if (g == null) Debug.Log("g to null");
            else Debug.Log("g to nie null");
            Vector3 lobbySpawn = GameObject.Find("LobbySpawn").transform.position;
            Vector3 randomStartPosition = new Vector3(lobbySpawn.x + Random.Range(-1, 1), lobbySpawn.y, lobbySpawn.z + Random.Range(-1, 1));
            
            Rigidbody rigidbody = GetComponent<Rigidbody>();
            rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            rigidbody.isKinematic = true;
            transform.position = new Vector3(randomStartPosition.x, randomStartPosition.y, randomStartPosition.z);
            rigidbody.isKinematic = false;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            
            transform.position = randomStartPosition;
        }
        
        GetComponent<PlayerManager>().Setup();
    }

    void EquipWeaponNotLocal() {
        Transform rightHand = transform.GetChild(0).GetChild(0).GetChild(2).GetChild(2)
            .GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(0).transform;
        GameObject weaponObject = Instantiate(_weaponObjectPrefab, rightHand);
        if (weaponObject.GetComponent<Weapon>().Name.Equals("Rifle")) {
            weaponObject.transform.localPosition = new Vector3(-0.061f, -0.442f, 0.308f);
            weaponObject.transform.localRotation = Quaternion.Euler(-106.591f, 62.645f, 25.95499f);
        }
        else {
            weaponObject.transform.localPosition = new Vector3(-0.115f, -0.407f, 0.333f);
            weaponObject.transform.localRotation = Quaternion.Euler(-101.359f, 19.54199f, 79.521f);
        }

        PlayerShoot shoot = GetComponent<PlayerShoot>();
        shoot.source = GetComponent<AudioSource>();
        shoot.Cam = _actionCamera;
        _equipment = GetComponent<PlayerEquipment>();
        _equipment.Weapon1 = weaponObject.GetComponent<Weapon>();
        _equipment.WeaponSound = weaponObject.GetComponent<AudioSource>();
        _equipment.Weapon1.GetComponent<Animator>().enabled = false;
        shoot.Equipment = _equipment;

        _actionCamera.transform.GetChild(1).GetComponent<Camera>().enabled = false;

        GameManager.SetLayerRecursively(rightHand.GetChild(5).gameObject, "LocalPlayer");
    }

    public void EquipWeapon() {
        GameObject weaponObject = Instantiate(_weaponObjectPrefab, _actionCamera.transform.GetChild(0));
        PlayerShoot shoot = GetComponent<PlayerShoot>();
        shoot.Cam = _actionCamera;
        _equipment = GetComponent<PlayerEquipment>();
        _equipment.Weapon1 = weaponObject.GetComponent<Weapon>();
        _equipment.WeaponSound = weaponObject.GetComponent<AudioSource>();
        shoot.Equipment = _equipment;
    }

    public override void OnStartClient() {
        base.OnStartClient();
        GameManager.RegisterPlayer(GetComponent<NetworkIdentity>().netId.ToString(), GetComponent<PlayerManager>());
    }

    private void AssignRemoteLayer() {
        transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("RemotePlayer");
    }

    private void DisableComponents() {
        for (int i = 0; i < _toDisable.Length; i++)
            _toDisable[i].enabled = false;
        _buildingCamera.gameObject.SetActive(false);
    }

    private void OnDisable() {
        if (GameManager.Players.ContainsKey(transform.name))
        {

            if (isLocalPlayer)
            {
                if (_sceneCamera != null)
                    _sceneCamera.gameObject.SetActive(true);
            }

            GameManager.UnregisterPlayer(transform.name);
        }
    }

    private void OnEnable() {
        if (isLocalPlayer) {
            if (_sceneCamera != null)
                _sceneCamera.gameObject.SetActive(false);
        }
    }
}