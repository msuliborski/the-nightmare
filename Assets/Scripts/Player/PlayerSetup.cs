using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(PlayerManager))]
public class PlayerSetup : NetworkBehaviour
{
    [SerializeField] private Behaviour[] _toDisable;
    [SerializeField] private Camera _sceneCamera;
    [SerializeField] private Camera _cam;
    [SerializeField] private GameObject _weaponObjectPrefab;
    
    // Start is called before the first frame update
    void Start()
    {
        EquipWeapon();

        if (!isLocalPlayer)
        {
            DisableComponents();
            AssignRemoteLayer();
            DisableWeaponCamera();
        }
        else
        {
           _sceneCamera = Camera.main;
           if (_sceneCamera != null)
                _sceneCamera.gameObject.SetActive(false);
        }

        GetComponent<PlayerManager>().Setup();
    }

    void DisableWeaponCamera()
    {
        _cam.transform.GetChild(1).GetComponent<Camera>().enabled = false;
    }

    void EquipWeapon()
    {
        GameObject weaponObject = Instantiate(_weaponObjectPrefab, _cam.transform.GetChild(0));
        PlayerShoot shoot = GetComponent<PlayerShoot>();
        shoot.Cam = _cam;
        PlayerEquipment equipment = GetComponent<PlayerEquipment>();
        equipment.Weapon = weaponObject.GetComponent<PlayerWeapon>();
        equipment.WeaponSound = weaponObject.GetComponent<AudioSource>();
        shoot.Equipment = equipment;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        GameManager.RegisterPlayer(GetComponent<NetworkIdentity>().netId.ToString(), GetComponent<PlayerManager>());
    }

    private void AssignRemoteLayer()
    {
        transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("RemotePlayer");
    }

    private void DisableComponents()
    {
        for (int i = 0; i < _toDisable.Length; i++)
            _toDisable[i].enabled = false;
    }

    private void OnDisable()
    {
        if (_sceneCamera != null)
            _sceneCamera.gameObject.SetActive(true);

        GameManager.UnregisterPlayer(transform.name);
    }
}
