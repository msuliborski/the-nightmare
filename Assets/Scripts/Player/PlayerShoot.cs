using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerShoot : NetworkBehaviour
{
    [SerializeField] private GameObject _weaponObjectPrefab;
    private AudioSource _weaponSound;
    private PlayerWeapon _weapon;
    [SerializeField] private Camera _cam;
    [SerializeField] private LayerMask _mask;
    
    // Start is called before the first frame update
    void Start()
    {
        if (_cam == null) enabled = false;
        else
        {
            GameObject weaponObject = Instantiate(_weaponObjectPrefab, _cam.transform);
            _weapon = weaponObject.GetComponent<PlayerWeapon>();
            _weaponSound = weaponObject.GetComponent<AudioSource>();
        }
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        _weaponSound.Play();
        RaycastHit hit;
        if (Physics.Raycast(_cam.transform.position, _cam.transform.forward, out hit, _weapon.Range, _mask))
        {
            Debug.Log("We hit " + hit.collider.name);
            if (hit.collider.tag == "Player")
                CmdPlayerShoot(hit.collider.name);
        }
    }

    [Command]
    void CmdPlayerShoot(string playerId)
    {
        Debug.Log(playerId + " has been shoot");
    }
}
