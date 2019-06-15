using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AI;

public class PlayerShoot : NetworkBehaviour {
    public Camera Cam { get; set; }

    public PlayerEquipment Equipment { get; set; }
    private PlayerController _playerController;
    private List<Material> _originalMaterials;
    [SerializeField] Material _blackeningMaterial;
    private bool _blackened = false;
    public bool Blackened {
        get { return _blackened;  }
        set
        {
            Transform weaponModelTransform = Equipment.getActiveWeapon().transform.transform.GetChild(0).GetChild(0).GetChild(1);

            if (value)
            {
                _originalMaterials = new List<Material>();
                for (int i = 0; i < weaponModelTransform.childCount; i++)
                {
                    MeshRenderer meshRenderer = weaponModelTransform.GetChild(i).GetComponent<MeshRenderer>();
                    _originalMaterials.Add(meshRenderer.material);
                    meshRenderer.material = _blackeningMaterial;
                }
            }
            else
            {
                for (int i = 0; i < weaponModelTransform.childCount; i++)
                {
                    MeshRenderer meshRenderer = weaponModelTransform.GetChild(i).GetComponent<MeshRenderer>();
                    meshRenderer.material = _originalMaterials[i];
                }
            }
        }
    }


    public GameObject Cross;

    [SerializeField] private LayerMask _mask;
    private bool _shootingDone = false;
    private float crossAccuracy = 1;
    private float normalFOV;
    private float zoomFOV;
    [SerializeField] private GameObject _grenadePrefab;
    private float _grenadeTimer = 0f;
    public int _maxGrenades = 3;
    public int _grenades;

    public TextMeshProUGUI _grenadesTM;


    public bool IsBuildingOnFly { get; set; }
    public bool WasBuilt { get; set; }
    private Animator weaponAnimator;
    private static readonly int IsAiming = Animator.StringToHash("isAiming");
    private static readonly int IsReloading = Animator.StringToHash("isReloading");
    private static readonly int IsSprinting = Animator.StringToHash("isSprinting");
    private float currentRecoil;
    private float changeWeaponCooldown = 0;
    private Weapon activeWeapon = null;


    // Start is called before the first frame update
    void Start() {
        activeWeapon = Equipment.Weapon1;
        if (Cam == null) enabled = false;
        else {
            Cross = GameObject.Find("cross");
            IsBuildingOnFly = false;
        }

        _playerController = GetComponent<PlayerController>();

        _grenades = _maxGrenades;
        if (isLocalPlayer)
            _grenadesTM = GameObject.Find("Grenades").GetComponent<TextMeshProUGUI>();
        normalFOV = Cam.fieldOfView;
        zoomFOV = normalFOV - 40;
        currentRecoil = activeWeapon.Recoil;

        if (!isLocalPlayer) activeWeapon.GetComponent<Animator>().enabled = false;
    }


    void Update() {
        
        currentRecoil = Equipment.getActiveWeapon().Recoil;
//        weaponAnimator = activeWeapon.GetComponent<Animator>();
        
        if (_grenades > _maxGrenades)
            _grenades = _maxGrenades;
        if (isLocalPlayer)
            _grenadesTM.text = "x" + _grenades;
        if (crossAccuracy > 1.02) crossAccuracy -= (crossAccuracy * 0.05f + 0.02f);
        else crossAccuracy = 1f;
        Cross.transform.GetChild(0).transform.localScale = new Vector3(crossAccuracy, crossAccuracy, crossAccuracy);
        
        //changing weapon
        if (changeWeaponCooldown > 0) changeWeaponCooldown -= Time.deltaTime;
        if (changeWeaponCooldown <= 0 && Math.Abs(Input.GetAxis("Mouse ScrollWheel")) > 0.01 && Equipment.getActiveWeapon().State == Weapon.WeaponState.idle) {
            if (Equipment.Weapon2 != null) {
                if (Blackened) Blackened = false;
                if (Equipment.Weapon1.gameObject.activeSelf) {
                    Equipment.Weapon1.transform.localPosition = new Vector3(0.02f, 0.03f, -0.22f);
                    Equipment.Weapon1.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                    Equipment.Weapon1.gameObject.SetActive(false);
                    Equipment.Weapon2.gameObject.SetActive(true);
                }
                else {
                    Equipment.Weapon1.gameObject.SetActive(true);
                    Equipment.Weapon2.transform.localPosition = new Vector3(0.08f, -0.02f, -0.17f);
                    Equipment.Weapon2.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                    Equipment.Weapon2.gameObject.SetActive(false);
                }
            }
        }
            
        Debug.Log(Input.GetAxis("Mouse ScrollWheel"));
        //fire mode
        if (Input.GetKeyDown(KeyCode.B)) Equipment.getActiveWeapon().changeFireMode();

        //reloading
        if (Input.GetKeyDown(KeyCode.R) && Equipment.getActiveWeapon().CurrentMagAmmo != Equipment.getActiveWeapon().MaxMagAmmo) {
            StartCoroutine(Reload());
        }

        if (_grenades > 0) {
            if (Input.GetKeyUp(KeyCode.G)) {
                _grenades--;
                CmdSpawnGrenade(transform.position, transform.rotation, transform.forward, (_grenadeTimer + 0.5f) / 3);
                _grenadeTimer = 0f;
            }
            else if (Input.GetKey(KeyCode.G)) {
                if (_grenadeTimer <= 3f) {
                    _grenadeTimer += Time.deltaTime;
                }
            }
        }

        //fireing
        if (Input.GetButton("Fire1") && Equipment.getActiveWeapon().State == Weapon.WeaponState.idle &&
            !PauseGame.menuActive && Equipment.getActiveWeapon().CurrentMagAmmo >= 1 && !IsBuildingOnFly) {
            Shoot();
        }

        if (Input.GetButtonUp("Fire1")) {
            _shootingDone = false;
            if (WasBuilt) {
                IsBuildingOnFly = false;
                WasBuilt = false;
            }
        }

        //aiming
        if (Input.GetButton("Fire2") &&
            (Equipment.getActiveWeapon().State == Weapon.WeaponState.idle ||
             Equipment.getActiveWeapon().State == Weapon.WeaponState.shooting) &&
            !PauseGame.menuActive) {
            currentRecoil = Equipment.getActiveWeapon().Recoil * 0.35f;
            if (isLocalPlayer) {
                Equipment.getActiveWeapon().GetComponent<Animator>().SetBool(IsSprinting, false);
                Equipment.getActiveWeapon().GetComponent<Animator>().SetBool(IsAiming, true);
            }

            Cam.fieldOfView = Mathf.Lerp(Cam.fieldOfView, zoomFOV, 0.6f);
            Cross.gameObject.SetActive(false);
            _playerController.SensitivityScale = _playerController.ZoomSensitivity;
        }
        else {
            currentRecoil = Equipment.getActiveWeapon().Recoil;
            if (isLocalPlayer) Equipment.getActiveWeapon().GetComponent<Animator>().SetBool(IsAiming, false);
            Cam.fieldOfView = Mathf.Lerp(Cam.fieldOfView, normalFOV, 0.6f);
            Cross.gameObject.SetActive(true);
            _playerController.SensitivityScale = _playerController.NonZoomSensitivity;
        }
    }


    IEnumerator Reload() {
        if (isLocalPlayer) Equipment.getActiveWeapon().GetComponent<Animator>().SetBool(IsReloading, true);
        Blackened = true;
        Equipment.getActiveWeapon().reload();
        yield return new WaitForSeconds(Equipment.getActiveWeapon().ReloadTime);
        Blackened = false;
        if (isLocalPlayer) Equipment.getActiveWeapon().GetComponent<Animator>().SetBool(IsReloading, false);
    }

    IEnumerator TripleShot() {
        PerformWeaponFire();
        yield return new WaitForSeconds(Equipment.getActiveWeapon().FireRate * 0.8f);
        PerformWeaponFire();
        yield return new WaitForSeconds(Equipment.getActiveWeapon().FireRate * 0.8f);
        PerformWeaponFire();
        yield return new WaitForSeconds(Equipment.getActiveWeapon().FireRate * 0.8f);
    }


    void Shoot() {
        crossAccuracy += 2f - crossAccuracy * 0.5f;
        Equipment.getActiveWeapon().GetComponent<Animator>().SetBool(IsSprinting, false);


        if (Equipment.getActiveWeapon().Mode == Weapon.FireMode.single && !_shootingDone) {
            PerformWeaponFire();
            _shootingDone = true;
        }
        else if (Equipment.getActiveWeapon().Mode == Weapon.FireMode.triple && !_shootingDone) {
            Equipment.getActiveWeapon().Recoil = Equipment.getActiveWeapon().Recoil / 2;
            StartCoroutine(TripleShot());
            Equipment.getActiveWeapon().Recoil = Equipment.getActiveWeapon().Recoil * 2;
            _shootingDone = true;
        }
        else if (Equipment.getActiveWeapon().Mode == Weapon.FireMode.continous) {
            PerformWeaponFire();
        }
    }


    void PerformWeaponFire() {
        if (Equipment.getActiveWeapon().CurrentMagAmmo >= 1) {
            Equipment.PlayerShooting();
            Equipment.getActiveWeapon().shoot();
            gameObject.GetComponent<PlayerMotor>().IncreaseRecoil(currentRecoil);
            CmdPlayerShooting();
            RaycastHit hit;
            if (Physics.Raycast(Cam.transform.position, Cam.transform.forward, out hit, Equipment.getActiveWeapon().Range,
                _mask)) {
                Debug.Log("We hit " + hit.collider.name);
                if (hit.collider.tag == "Player") { //wylaczamy friendly fire???
                    StartCoroutine(ShowHitmarker());
                    CmdPlayerShoot(hit.collider.GetComponentInParent<PlayerManager>().transform.name,
                        Equipment.getActiveWeapon().Damage);
                }
                else if (hit.collider.tag == "EnemyHead") {
                    StartCoroutine(ShowHitmarker());
                    CmdEnemyShoot(hit.collider.GetComponentInParent<NavMeshAgent>().transform.name,
                        3 * Equipment.getActiveWeapon().Damage);
                }
                else if (hit.collider.tag == "EnemyBody") {
                    StartCoroutine(ShowHitmarker());
                    CmdEnemyShoot(hit.collider.GetComponentInParent<NavMeshAgent>().transform.name,
                        2 * Equipment.getActiveWeapon().Damage);
                }
                else if (hit.collider.tag == "EnemyLegs") {
                    StartCoroutine(ShowHitmarker());
                    CmdEnemyShoot(hit.collider.GetComponentInParent<NavMeshAgent>().transform.name,
                        Equipment.getActiveWeapon().Damage);
                }


                Equipment.DoHitEffect(hit.point, hit.normal);
                CmdOnHit(hit.point, hit.normal);
            }

            if (Equipment.getActiveWeapon().CurrentMagAmmo == 0 && Equipment.getActiveWeapon().CurrentAmmo >= 1) StartCoroutine(Reload());
        }
    }

    IEnumerator ShowHitmarker() {
        //play hit sound
        Cross.transform.GetChild(1).gameObject.SetActive(true);
        yield return new WaitForSeconds(0.15f);
        Cross.transform.GetChild(1).gameObject.SetActive(false);
    }

    [Command]
    void CmdPlayerShooting() {
        Equipment.RpcPlayerShooting();
    }

    [Command]
    void CmdOnHit(Vector3 hitPoint, Vector3 normal) {
        Equipment.RpcDoHitEffect(hitPoint, normal);
    }

    [Command]
    void CmdEnemyShoot(string shootEnemyId, float damage) {
        Debug.Log(shootEnemyId + " has been shoot");
        GameManager.GetEnemy(shootEnemyId).CmdTakeDamage(damage);
    }


    public void InvokeCmdPlayerShoot(string shootPlayerId, float damage) {
        CmdPlayerShoot(shootPlayerId, damage);
    }

    [Command]
    void CmdPlayerShoot(string shootPlayerId, float damage) {
        Debug.Log(shootPlayerId + " has been shoot");
        GameManager.GetPlayer(shootPlayerId).RpcTakeDamage(damage);
    }

    [Command]
    void CmdSpawnGrenade(Vector3 playerPos, Quaternion playerRot, Vector3 forwardVector, float force) {
        GameObject grenade = Instantiate(_grenadePrefab, playerPos + forwardVector + Vector3.up / 2, playerRot);
        Rigidbody rb = grenade.GetComponent<Rigidbody>();
        rb.velocity = force * (15 * forwardVector + 5 * Vector3.up);
        NetworkServer.Spawn(grenade);
    }
}