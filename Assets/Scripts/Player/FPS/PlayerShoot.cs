using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AI;

public class PlayerShoot : NetworkBehaviour {
    public Camera Cam { get; set; }

    public PlayerEquipment Equipment { get; set; }

    public GameObject Cross;

    [SerializeField] private LayerMask _mask;
    private bool _shootingDone = false;
    private float crossAccuracy = 1;
    private float normalFOV;
    private float zoomFOV;

    public bool IsBuildingOnFly { get; set; }
    public bool WasBuilt { get; set; }
    private Animator weaponAnimator;
    private static readonly int IsAiming = Animator.StringToHash("isAiming");
    private static readonly int IsReloading = Animator.StringToHash("isReloading");
    private static readonly int IsSprinting = Animator.StringToHash("isSprinting");
    private float currentRecoil;
    
    


    // Start is called before the first frame update
    void Start() {
        if (Cam == null) enabled = false;
        else {
            Cross = GameObject.Find("cross");
            IsBuildingOnFly = false;
        }

        normalFOV = Cam.fieldOfView;
        zoomFOV = normalFOV - 40;
        currentRecoil = Equipment.Weapon.Recoil;

        if (!isLocalPlayer) Equipment.Weapon.GetComponent<Animator>().enabled = false;
    }


    void Update() {
        if (crossAccuracy > 1.02) crossAccuracy -= (crossAccuracy * 0.05f + 0.02f);
        else crossAccuracy = 1f;
        Cross.transform.localScale = new Vector3(crossAccuracy, crossAccuracy, crossAccuracy);
        //fire mode
        if (Input.GetKeyDown(KeyCode.B)) Equipment.Weapon.changeFireMode();

        //reloading
        if (Input.GetKeyDown(KeyCode.R) && Equipment.Weapon.CurrentMagAmmo != Equipment.Weapon.MaxMagAmmo) {
            StartCoroutine(Reload());
        }

        //fireing
        if (Input.GetButton("Fire1") && Equipment.Weapon.State == Weapon.WeaponState.idle &&
            !PauseGame.menuActive && Equipment.Weapon.CurrentMagAmmo >= 1 && !IsBuildingOnFly) {
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
            (Equipment.Weapon.State == Weapon.WeaponState.idle ||
             Equipment.Weapon.State == Weapon.WeaponState.shooting) &&
            !PauseGame.menuActive) {
            currentRecoil = Equipment.Weapon.Recoil * 0.35f;
            if (isLocalPlayer)
            {
                Equipment.Weapon.GetComponent<Animator>().SetBool(IsSprinting, false);
                Equipment.Weapon.GetComponent<Animator>().SetBool(IsAiming, true);
            }
            Cam.fieldOfView = Mathf.Lerp(Cam.fieldOfView, zoomFOV, 0.6f);
            Cross.gameObject.SetActive(false);
        }
        else {
            currentRecoil = Equipment.Weapon.Recoil;
            if (isLocalPlayer) Equipment.Weapon.GetComponent<Animator>().SetBool(IsAiming, false);
            Cam.fieldOfView = Mathf.Lerp(Cam.fieldOfView, normalFOV, 0.6f);
            Cross.gameObject.SetActive(true);
        }
    }


    IEnumerator Reload() {
        if (isLocalPlayer) Equipment.Weapon.GetComponent<Animator>().SetBool(IsReloading, true);
        Equipment.Weapon.reload();
        yield return new WaitForSeconds(Equipment.Weapon.ReloadTime);
        if (isLocalPlayer) Equipment.Weapon.GetComponent<Animator>().SetBool(IsReloading, false);
    }

    IEnumerator TripleShot() {
        PerformWeaponFire();
        yield return new WaitForSeconds(Equipment.Weapon.FireRate * 0.8f);
        PerformWeaponFire();
        yield return new WaitForSeconds(Equipment.Weapon.FireRate * 0.8f);
        PerformWeaponFire();
        yield return new WaitForSeconds(Equipment.Weapon.FireRate * 0.8f);
    }


    void Shoot()
    {
        crossAccuracy += 2f - crossAccuracy * 0.5f;
                Equipment.Weapon.GetComponent<Animator>().SetBool(IsSprinting, false);


        if (Equipment.Weapon.Mode == Weapon.FireMode.single && !_shootingDone) {
            PerformWeaponFire();
            _shootingDone = true;
        }
        else if (Equipment.Weapon.Mode == Weapon.FireMode.triple && !_shootingDone) {
            Equipment.Weapon.Recoil = Equipment.Weapon.Recoil / 2;
            StartCoroutine(TripleShot());
            Equipment.Weapon.Recoil = Equipment.Weapon.Recoil * 2;
            _shootingDone = true;
        }
        else if (Equipment.Weapon.Mode == Weapon.FireMode.continous) {
            PerformWeaponFire();
        }
    }


    void PerformWeaponFire() {
        if (Equipment.Weapon.CurrentMagAmmo >= 1) {
            Equipment.PlayerShooting();
            Equipment.Weapon.shoot();
            gameObject.GetComponent<PlayerMotor>().IncreaseRecoil(currentRecoil);
            CmdPlayerShooting();
            RaycastHit hit;
            if (Physics.Raycast(Cam.transform.position, Cam.transform.forward, out hit, Equipment.Weapon.Range,
                _mask)) {
                Debug.Log("We hit " + hit.collider.name);
                if (hit.collider.tag == "Player")
                    CmdPlayerShoot(hit.collider.GetComponentInParent<PlayerManager>().transform.name,
                        Equipment.Weapon.Damage);
                else if (hit.collider.tag == "EnemyHead")
                    CmdEnemyShoot(hit.collider.GetComponentInParent<NavMeshAgent>().transform.name,
                        3 * Equipment.Weapon.Damage);
                else if (hit.collider.tag == "EnemyBody")
                    CmdEnemyShoot(hit.collider.GetComponentInParent<NavMeshAgent>().transform.name,
                        2 * Equipment.Weapon.Damage);
                else if (hit.collider.tag == "EnemyLegs")
                    CmdEnemyShoot(hit.collider.GetComponentInParent<NavMeshAgent>().transform.name,
                        Equipment.Weapon.Damage);

                Equipment.DoHitEffect(hit.point, hit.normal);
                CmdOnHit(hit.point, hit.normal);
            }

            if (Equipment.Weapon.CurrentMagAmmo == 0 && Equipment.Weapon.CurrentAmmo >= 1) StartCoroutine(Reload());
        }
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
}