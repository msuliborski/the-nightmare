
using UnityEngine;
using UnityEngine.Networking;

public class PlayerEquipment : NetworkBehaviour {
    public AudioSource WeaponSound { get; set; }
    public Weapon Weapon { get; set; }
    [SerializeField] private Camera _cam;
    [SerializeField] private LayerMask _mask;

    private void Update() {
       RaycastHit weaponFinder;
        if (Physics.Raycast(_cam.transform.position, _cam.transform.forward, out weaponFinder, 10,
            _mask)) {
            if (weaponFinder.collider.CompareTag("Weapon") && Input.GetKeyDown(KeyCode.E)){
                Destroy(_cam.transform.GetChild(0).transform.GetChild(0).gameObject);
                int weaponId = weaponFinder.collider.gameObject.GetComponent<GunSpawnPoint>().WeaponId;
                GameObject weaponObject = Instantiate(GameManager.Instance.Weapons[weaponId], _cam.transform.GetChild(0).transform);
                Weapon = weaponObject.GetComponent<Weapon>();
                WeaponSound = weaponObject.GetComponent<AudioSource>();
                CmdChangeWeapon(weaponId);
            }
        }
        
        
        Transform rightHand = transform.GetChild(0).GetChild(0).GetChild(2).GetChild(2)
            .GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(0).transform;
        if (rightHand.GetChild(5).gameObject.GetComponent<Weapon>().Name.Equals("Rifle")) {
            Debug.Log("rifle");
            rightHand.GetChild(5).transform.localPosition = new Vector3(-0.061f, -0.442f, 0.308f);
            rightHand.GetChild(5).transform.localRotation = Quaternion.Euler(-106.591f, 62.645f, 25.95499f);
        } else {
            Debug.Log("pistol");
            rightHand.GetChild(5).transform.localPosition = new Vector3(-0.115f, -0.407f, 0.333f);
            rightHand.GetChild(5).transform.localRotation = Quaternion.Euler(-101.359f, 19.54199f, 79.521f);
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
        if (!isLocalPlayer) {
            Transform rightHand = transform.GetChild(0).GetChild(0).GetChild(2).GetChild(2)
                .GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(0).transform;
            Destroy(rightHand.GetChild(5).gameObject);
            GameObject weaponObject = Instantiate(GameManager.Instance.Weapons[weaponId], rightHand); //_cam.transform.GetChild(0).transform);//tutaj
            if (weaponObject.GetComponent<Weapon>().Name.Equals("Rifle")) {
                Debug.Log("rifla wzial");
                weaponObject.transform.localPosition = new Vector3(-0.061f, -0.442f, 0.308f);
                weaponObject.transform.localRotation = Quaternion.Euler(-106.591f, 62.645f, 25.95499f);
            } else {
                Debug.Log("pistola wzial");
                weaponObject.transform.localPosition = new Vector3(-0.115f, -0.407f, 0.333f);
                weaponObject.transform.localRotation = Quaternion.Euler(-101.359f, 19.54199f, 79.521f);
            }  
            Weapon = weaponObject.GetComponent<Weapon>();
            WeaponSound = weaponObject.GetComponent<AudioSource>();
            Weapon.State = Weapon.WeaponState.idle;
            Weapon.CurrentMagAmmo = Weapon.MaxMagAmmo;
            Weapon.CurrentAmmo = Weapon.MaxAmmo;
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
