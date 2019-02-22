using UnityEngine;
using UnityEngine.Networking;

public class WeaponBlocked : NetworkBehaviour
{
    private bool _isBlocked = false;
    public bool IsBlocked
    {
        get { return _isBlocked; }
        set
        {
            _isBlocked = value;
            if (value)
            {
                if (_cross != null)
                {
                    _cross.SetActive(false);
                    _shoot.enabled = false;
                }
                _gun.transform.localRotation = Quaternion.Euler(-75f, 0f, 0f);
                //_gun.transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, _initPosZ + 0.4f);
            }
            else
            {
                if (_cross != null)
                {
                    _cross.SetActive(true);
                    _shoot.enabled = true;
                }
                _gun.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                //_gun.transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, _initPosZ);
            }
        }
    }
    [SerializeField] private GameObject _cross;
    [SerializeField] private PlayerManager _player;
    private Transform _gun;
    private float _initPosZ; 
    private PlayerShoot _shoot;
     

    private void Start()
    {

        _initPosZ = transform.localPosition.z;
        _player = transform.GetComponentInParent<PlayerManager>();
        _gun = transform.GetChild(0);
        
        if (_player.isLocalPlayer)
        { 
            _cross = GameObject.Find("cross");
            _shoot = transform.GetComponentInParent<PlayerShoot>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Floor")) IsBlocked = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Floor")) IsBlocked = false; 
    }
}
