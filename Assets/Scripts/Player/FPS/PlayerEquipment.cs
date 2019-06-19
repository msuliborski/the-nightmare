using System;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerEquipment : NetworkBehaviour {
    public AudioSource WeaponSound { get; set; }
    public Weapon Weapon1 { get; set; }
    public Weapon Weapon2 { get; set; }
    [SerializeField] private Camera _cam;
    [SerializeField] private LayerMask _mask;
    public GameObject pickUp;
    private PlayerShoot _shoot;
    private PlacementController _controller;
    private Chest _chest;
    private ChestAlwaysFull _chestAlwaysFull;

    private void Start() {
        if (isLocalPlayer) {
            pickUp = GameObject.Find("PickUp");
            pickUp.SetActive(false);
        }
        else {
            transform.GetChild(4).gameObject.SetActive(false); //turn off camera
        }

        _shoot = GetComponent<PlayerShoot>();
        _controller = GetComponent<PlacementController>();
        Weapon2 = null;
    }

    public Weapon getActiveWeapon() {
        if (Weapon1 == null) return null;
        if (Weapon1.gameObject.activeSelf)
            return Weapon1;
        return Weapon2;
    }

    private void Update() {
        RaycastHit weaponFinder;
        if (Physics.Raycast(_cam.transform.position, _cam.transform.forward, out weaponFinder, 0.75f,
            _mask)) {
            if (weaponFinder.collider.CompareTag("removableChairs")) {
                if (isLocalPlayer) {
                    pickUp.SetActive(true);
                    pickUp.transform.GetComponent<TextMeshProUGUI>().text = "Press E to Remove Chairs";
                }

                if (Input.GetKeyDown(KeyCode.E)) {
                    Destroy(weaponFinder.collider.gameObject);
                }
            }
            else if (weaponFinder.collider.CompareTag("Weapon")) {
                if (isLocalPlayer){
                    pickUp.SetActive(true);
                    pickUp.transform.GetComponent<TextMeshProUGUI>().text = "Press E to pick up Rifle";
                }
                if (Input.GetKeyDown(KeyCode.E)) {
                    if (_cam.transform.GetChild(0).transform.childCount <= 1) {
                        int weaponId = 1;
                        if (weaponId != 0) {
                            GameObject weaponObject = Instantiate(GameManager.Instance.Weapons[weaponId],
                                _cam.transform.GetChild(0).transform);
                            Weapon2 = weaponObject.GetComponent<Weapon>();
                            Weapon1.transform.localPosition = new Vector3(0.02f, 0.03f, -0.22f);
                            Weapon1.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                            Weapon1.gameObject.SetActive(false);
                            CmdChangeWeapon(weaponId);
                        }
                    }
                    else {
                        int weaponId = weaponFinder.collider.gameObject.GetComponent<GunSpawnPoint>().WeaponId;
                        if (weaponId == 1) {
                            Weapon2.CurrentAmmo = Weapon2.MaxAmmo;
                            Weapon2.CurrentMagAmmo = Weapon2.MaxMagAmmo;
                        }
                    }
                }
            }
            else if (weaponFinder.collider.CompareTag("Chest")) {
                _chest = weaponFinder.collider.GetComponentInParent<Chest>();
                _chestAlwaysFull = weaponFinder.collider.GetComponentInParent<ChestAlwaysFull>();
                if (_chestAlwaysFull == null) {
                    if (_chest.active && !_chest.alreadyPicked) {
                        if (isLocalPlayer){
                            pickUp.SetActive(true);
                            pickUp.transform.GetComponent<TextMeshProUGUI>().text = "Press E to pick up collectibles";
                        }
                        if (Input.GetKeyDown(KeyCode.E)) {
                            _shoot._grenades += _chest.grenades;
                            if (_controller.placeableCount[0] + _chest.snares >= _controller.maxPlaceable[0])
                                _controller.placeableCount[0] = _controller.maxPlaceable[0];
                            else
                                _controller.placeableCount[0] += _chest.snares;

                            if (_controller.placeableCount[1] + _chest.teddyBears >= _controller.maxPlaceable[1])
                                _controller.placeableCount[1] = _controller.maxPlaceable[1];
                            else
                                _controller.placeableCount[1] += _chest.teddyBears;

                            if (_controller.placeableCount[2] + _chest.barrels >= _controller.maxPlaceable[2])
                                _controller.placeableCount[2] = _controller.maxPlaceable[2];
                            else
                                _controller.placeableCount[2] += _chest.barrels;
                            _chest.alreadyPicked = true;
                        }
                    }
                    else {
                        if (isLocalPlayer)
                            pickUp.SetActive(false);
                    }

                }
                else {
                    if (_chestAlwaysFull.active && !_chestAlwaysFull.alreadyPicked) {
                        if (isLocalPlayer){
                            pickUp.SetActive(true);
                            pickUp.transform.GetComponent<TextMeshProUGUI>().text = "Press E to pick up collectibles";
                        }
                        if (Input.GetKeyDown(KeyCode.E)) {
                            _shoot._grenades += _chestAlwaysFull.grenades;
                            if (_controller.placeableCount[0] + _chestAlwaysFull.snares >= _controller.maxPlaceable[0])
                                _controller.placeableCount[0] = _controller.maxPlaceable[0];
                            else
                                _controller.placeableCount[0] += _chestAlwaysFull.snares;

                            if (_controller.placeableCount[1] + _chestAlwaysFull.teddyBears >= _controller.maxPlaceable[1])
                                _controller.placeableCount[1] = _controller.maxPlaceable[1];
                            else
                                _controller.placeableCount[1] += _chestAlwaysFull.teddyBears;

                            if (_controller.placeableCount[2] + _chestAlwaysFull.barrels >= _controller.maxPlaceable[2])
                                _controller.placeableCount[2] = _controller.maxPlaceable[2];
                            else
                                _controller.placeableCount[2] += _chestAlwaysFull.barrels;
                            _chestAlwaysFull.alreadyPicked = true;
                        }
                    }
                    else {
                        if (isLocalPlayer)
                            pickUp.SetActive(false);
                    }
                }
            }
            else {
                if (isLocalPlayer)
                    pickUp.SetActive(false);
            }
        }
        else {
            if (isLocalPlayer)
                pickUp.SetActive(false);
        }
    }

    public void PlayerShooting() {
        getActiveWeapon().Flash.Play();
        GameObject smokeEffect =
            Instantiate(getActiveWeapon().Smoke, getActiveWeapon().transform.GetChild(0).position,
                Quaternion.Euler(-90, 0, 0));
        Destroy(smokeEffect, 2f);
    }


    [Command]
    void CmdChangeWeapon(int weaponId) {
        RpcChangeWeapon(weaponId);
    }

    [ClientRpc]
    void RpcChangeWeapon(int weaponId) {
        if (!isLocalPlayer) {
            Transform rightHand = transform.GetChild(0).GetChild(0).GetChild(2).GetChild(2)
                .GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(0).transform;
//            Destroy(rightHand.GetChild(5).gameObject);
            GameObject weaponObject =
                Instantiate(GameManager.Instance.Weapons[weaponId],
                    rightHand); //_cam.transform.GetChild(0).transform);//tutaj
            if (weaponObject.GetComponent<Weapon>().Name.Equals("Rifle")) {
                weaponObject.transform.localPosition = new Vector3(-0.061f, -0.442f, 0.308f);
                weaponObject.transform.localRotation = Quaternion.Euler(-106.591f, 62.645f, 25.95499f);
            }
            else {
                weaponObject.transform.localPosition = new Vector3(-0.115f, -0.407f, 0.333f);
                weaponObject.transform.localRotation = Quaternion.Euler(-101.359f, 19.54199f, 79.521f);
            }

            Weapon2 = weaponObject.GetComponent<Weapon>();
            WeaponSound = weaponObject.GetComponent<AudioSource>();
            Weapon2.State = Weapon.WeaponState.idle;
            Weapon2.CurrentMagAmmo = Weapon1.MaxMagAmmo;
            Weapon2.CurrentAmmo = Weapon1.MaxAmmo;
            weaponObject.GetComponent<Animator>().enabled = false;
            GameManager.SetLayerRecursively(weaponObject, "LocalPlayer");
        }
    }

    [ClientRpc]
    public void RpcPlayerShooting() {
        if (!isLocalPlayer) {
            getActiveWeapon().Flash.Play();
            getActiveWeapon().GetComponent<AudioSource>().Play();
            GameObject smokeEffect = Instantiate(getActiveWeapon().Smoke,
                getActiveWeapon().transform.GetChild(0).position,
                Quaternion.Euler(-90, 0, 0));
            Destroy(smokeEffect, 2f);
        }
    }

    public void DoHitEffect(Vector3 hitPoint, Vector3 normal) {
        GameObject hitEffect = Instantiate(getActiveWeapon().HitEffect, hitPoint, Quaternion.LookRotation(normal));
        Destroy(hitEffect, 2f);
    }


    [ClientRpc]
    public void RpcDoHitEffect(Vector3 hitPoint, Vector3 normal) {
        if (!isLocalPlayer) {
            GameObject hitEffect = Instantiate(getActiveWeapon().HitEffect, hitPoint, Quaternion.LookRotation(normal));
            Destroy(hitEffect, 2f);
        }
    }
}