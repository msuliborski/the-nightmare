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
                // Don't hand the gun back to a player who died while pressed against the wall.
                if (_cross != null && !_player.IsDead)
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
    // Number of blocking colliders currently overlapping; leaving one of several used to unblock.
    private int _overlaps;
     

    private void Start()
    {

        _initPosZ = transform.localPosition.z;
        _player = transform.GetComponentInParent<PlayerManager>();
        _gun = transform.GetChild(0);
        if (_player.isLocalPlayer)
        {
            _shoot = transform.GetComponentInParent<PlayerShoot>();
            _cross = _shoot.Cross;
        }
    }

    private static bool Blocks(Collider other)
    {
        return !other.CompareTag("Floor") && !other.CompareTag("Enemy") &&
               !other.CompareTag("EnemyHead") && !other.CompareTag("EnemyLegs") &&
               !other.CompareTag("EnemyBody");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_gun == null || !Blocks(other)) return;
        _overlaps++;
        if (!_isBlocked) IsBlocked = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!Blocks(other)) return;
        _overlaps = Mathf.Max(0, _overlaps - 1);
        if (_overlaps == 0 && _isBlocked) IsBlocked = false;
    }

    // Colliders that get destroyed/disabled while overlapping never send OnTriggerExit.
    private void OnDisable()
    {
        _overlaps = 0;
        if (_isBlocked) IsBlocked = false;
    }
}
