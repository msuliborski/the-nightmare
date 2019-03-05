
using UnityEngine;
using UnityEngine.Networking;

public class PlayerEquipment : NetworkBehaviour {
    public AudioSource WeaponSound { get; set; }
    public PlayerWeapon Weapon { get; set; }
    [SerializeField] private Camera _cam;
    [SerializeField] private LayerMask _mask;

    private void Update() {
       RaycastHit weaponFinder;
        if (Physics.Raycast(_cam.transform.position, _cam.transform.forward, out weaponFinder, 10,
            _mask)) {
            if (weaponFinder.collider.CompareTag("Weapon") && Input.GetKeyDown(KeyCode.E))
            {
                Destroy(_cam.transform.GetChild(0).transform.GetChild(0).gameObject);
                int weaponId = weaponFinder.collider.gameObject.GetComponent<GunSpawnPoint>().WeaponId;
                GameObject weaponObject = Instantiate(GameManager.Instance.Weapons[weaponId], _cam.transform.GetChild(0).transform);
                Weapon = weaponObject.GetComponent<PlayerWeapon>();
                WeaponSound = weaponObject.GetComponent<AudioSource>();
                CmdChangeWeapon(weaponId);
            }
        }
    }


    public void PlayerShooting() {
        Weapon.Flash.Play();
        WeaponSound.Play();
        GameObject smokeEffect =
            Instantiate(Weapon.Smoke, Weapon.transform.GetChild(0).position, Quaternion.Euler(-90, 0, 0));
        Destroy(smokeEffect, 2f);
    }


    [Command]
    void CmdChangeWeapon(int weaponId)
    {
        RpcChangeWeapon(weaponId);
    }

    [ClientRpc]
    void RpcChangeWeapon(int weaponId)
    {
        if (!isLocalPlayer)
        {
            Destroy(_cam.transform.GetChild(0).transform.GetChild(0).gameObject);
            GameObject weaponObject = Instantiate(GameManager.Instance.Weapons[weaponId], _cam.transform.GetChild(0).transform);
            Weapon = weaponObject.GetComponent<PlayerWeapon>();
            WeaponSound = weaponObject.GetComponent<AudioSource>();
            GameManager.SetLayerRecursively(weaponObject, "LocalPlayer");
        }
    }

    [ClientRpc]
    public void RpcPlayerShooting() {
        if (!isLocalPlayer) {
            Weapon.Flash.Play();
            WeaponSound.Play();
            GameObject smokeEffect = Instantiate(Weapon.Smoke, Weapon.transform.GetChild(0).position,
                Quaternion.Euler(-90, 0, 0));
            Destroy(smokeEffect, 2f);
        }
    }

    public void DoHitEffect(Vector3 hitPoint, Vector3 normal) {
        GameObject hitEffect = Instantiate(Weapon.HitEffect, hitPoint, Quaternion.LookRotation(normal));
        Destroy(hitEffect, 2f);
    }


    [ClientRpc]
    public void RpcDoHitEffect(Vector3 hitPoint, Vector3 normal) {
        if (!isLocalPlayer) {
            GameObject hitEffect = Instantiate(Weapon.HitEffect, hitPoint, Quaternion.LookRotation(normal));
            Destroy(hitEffect, 2f);
        }
    }
}
