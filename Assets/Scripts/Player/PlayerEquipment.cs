
using UnityEngine;
using UnityEngine.Networking;

public class PlayerEquipment : NetworkBehaviour {
    public AudioSource WeaponSound { get; set; }
    public PlayerWeapon Weapon { get; set; }
    [SerializeField] private Camera _cam;
    [SerializeField] private LayerMask _mask;

    private void Update() {
       RaycastHit weaponFider;
        if (Physics.Raycast(_cam.transform.position, _cam.transform.forward, out weaponFider, 10,
            _mask)) {
            if (weaponFider.collider.CompareTag("Weapon") && Input.GetKeyDown(KeyCode.E)) {
                Weapon = weaponFider.collider.gameObject.GetComponent<GunSpawnPoint>().weapon.GetComponent<PlayerWeapon>();
                Debug.Log(weaponFider.collider.gameObject.GetComponent<GunSpawnPoint>().weapon.GetComponent<PlayerWeapon>().Name);
                
                Destroy(_cam.transform.GetChild(0).transform.GetChild(0).gameObject);
                GameObject weaponObject = Instantiate(weaponFider.collider.gameObject.GetComponent<GunSpawnPoint>().weapon.gameObject, _cam.transform.GetChild(0).transform);
                PlayerEquipment equipment = GetComponent<PlayerEquipment>();
                equipment.Weapon = weaponObject.GetComponent<PlayerWeapon>();
                equipment.WeaponSound = weaponObject.GetComponent<AudioSource>();
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
