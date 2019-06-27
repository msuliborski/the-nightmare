using System;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerEquipment : NetworkBehaviour {
    public AudioSource WeaponSound { get; set; }
    public Weapon Weapon1 { get; set; }
    public Weapon Weapon2 { get; set; }
    [SerializeField] private Camera _cam;
    [SerializeField] private LayerMask _mask;
    private TextMeshProUGUI pickUp;
    private PlayerShoot _shoot;
    private PlacementController _controller;
    private ChestAlwaysFull _chest;
    private AudioSource _source;
    [SerializeField] private AudioClip _pick;
    [SerializeField] private AudioClip _pickRifle;

    private void Start() {
        if (isLocalPlayer) {
            pickUp = GameObject.Find("PlayerUI").transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        }
        else {
            transform.GetChild(3).gameObject.SetActive(false);//turn off camera
            transform.GetChild(1).GetChild(2).gameObject.SetActive(false);//turn off arrow
        }
        
        _shoot = GetComponent<PlayerShoot>();
        _controller = GetComponent<PlacementController>();
        Weapon2 = null;
        _source = GetComponent<AudioSource>();
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
                    pickUp.enabled = true;
                    pickUp.text = "Press E to Remove Chairs";
                }

                if (Input.GetKeyDown(KeyCode.E)) {
                    Destroy(weaponFinder.collider.gameObject);
                }
            }
            else if (weaponFinder.collider.CompareTag("Weapon")) {
                if (isLocalPlayer){
                    pickUp.enabled = true;
                    pickUp.text = "Press E to pick up Rifle";
                }
                if (Input.GetKeyDown(KeyCode.E)) {
                    if (isLocalPlayer) {
                        _source.clip = _pickRifle;
                        _source.PlayOneShot(_source.clip);
                        if (_cam.transform.GetChild(0).transform.childCount <= 1) {
                            GameObject weaponObject = Instantiate(GameManager.Instance.Weapons[1],
                                _cam.transform.GetChild(0).transform);
                            Weapon2 = weaponObject.GetComponent<Weapon>();
                            Weapon1.transform.localPosition = new Vector3(0.02f, 0.03f, -0.22f);
                            Weapon1.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                            Weapon1.gameObject.SetActive(false);
//                            CmdChangeWeapon(1);
                        }
                        else {
                            Weapon2.CurrentAmmo = Weapon2.MaxAmmo + (Weapon2.MaxMagAmmo - Weapon2.CurrentMagAmmo);
//                        Weapon2.CurrentMagAmmo = Weapon2.MaxMagAmmo;
                        }
                    }
                    else {
                        Transform rightHand = transform.GetChild(0).GetChild(0).GetChild(2).GetChild(2)
                            .GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(0).transform;

                        GameObject weaponObject = null;
                        if (rightHand.childCount == 6) {
                            weaponObject = Instantiate(GameManager.Instance.Weapons[1], rightHand);
                            
                            PlayerShoot shoot = GetComponent<PlayerShoot>();
                            shoot.source = GetComponent<AudioSource>();
                            PlayerEquipment _equipment = GetComponent<PlayerEquipment>();
                            _equipment.Weapon2 = weaponObject.GetComponent<Weapon>();
                            _equipment.WeaponSound = weaponObject.GetComponent<AudioSource>();
                            _equipment.Weapon2.GetComponent<Animator>().enabled = false;
                            shoot.Equipment = _equipment;
                        }
                        else {
                            weaponObject = transform.GetComponent<PlayerEquipment>().Weapon2.gameObject;
//                            weaponObject.GetComponent<Weapon>().resetAmmo();
                            
                            weaponObject.GetComponent<Weapon>().CurrentAmmo = weaponObject.GetComponent<Weapon>().MaxAmmo + (weaponObject.GetComponent<Weapon>().MaxMagAmmo - weaponObject.GetComponent<Weapon>().CurrentMagAmmo);
                        }

                    }
                }
            }
            else if (weaponFinder.collider.CompareTag("reviving")) {
                if (isLocalPlayer){
                    pickUp.enabled = true;
                    pickUp.text = "Press E to revive";
                }
                if (Input.GetKeyDown(KeyCode.E)) {
                    CmdCallRevive(weaponFinder.collider.GetComponentInParent<PlayerManager>().transform.name);
                }
            }
            else if (weaponFinder.collider.CompareTag("Chest")) {
                _chest = weaponFinder.collider.GetComponentInParent<ChestAlwaysFull>();
                    if (_chest.active && !_chest.alreadyPicked) {
                        if (isLocalPlayer){
                            pickUp.enabled = true;
                            pickUp.text = "Press E to pick up collectibles";
                        }
                        if (Input.GetKeyDown(KeyCode.E))
                        {
                            _source.clip = _pick;
                            _source.PlayOneShot(_source.clip);
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
                            pickUp.enabled = false;
                    }
            }
            else {
                if (isLocalPlayer)
                    pickUp.enabled = false;
            }
        }
        else {
            if (isLocalPlayer)
                pickUp.enabled = false;
        }
    }

    [Command]
    private void CmdCallRevive(string name)
    {
        RpcCallRevive(name);
    }

    [ClientRpc]
    private void RpcCallRevive(string name)
    {
        GameManager.Players[name].Revive();
    }
    
    public void PlayerShooting() {
        getActiveWeapon().Flash.Play();
        GameObject smokeEffect =
            Instantiate(getActiveWeapon().Smoke, getActiveWeapon().transform.GetChild(0).position,
                Quaternion.Euler(-90, 0, 0));
        Destroy(smokeEffect, 2f);
    }


    [Command]
    public void CmdChangeWeapon(int weaponId) {
        RpcChangeWeapon(weaponId);
    }

    [ClientRpc]
    void RpcChangeWeapon(int weaponId) {
        if (!isLocalPlayer) {
            Transform rightHand = transform.GetChild(0).GetChild(0).GetChild(2).GetChild(2)
                .GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(0).transform;
            if (weaponId == 1) {
                rightHand.GetChild(5).gameObject.SetActive(false);
                rightHand.GetChild(6).gameObject.SetActive(true);
            } else {
                rightHand.GetChild(5).gameObject.SetActive(true);
                rightHand.GetChild(6).gameObject.SetActive(false);
            }

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
            _shoot.playSootSound();
            getActiveWeapon().Flash.Play();
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