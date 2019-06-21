using UnityEngine;
using UnityEngine.Networking;

public class Weapon : NetworkBehaviour {

    [SerializeField] protected int _id;
    public int Id { get { return _id; } set { _id = value; } }


    [SerializeField] protected string _name;
    public string Name {
        get { return _name; }
        set { _name = value; }
    }

    [SerializeField] protected int _damage;
    public int Damage {
        get { return _damage; }
        set { _damage = value; }
    }
    
    [SerializeField] protected int _currentMagAmmo;
    public int CurrentMagAmmo {
        get { return _currentMagAmmo; }
        set { _currentMagAmmo = value; }
    }

    [SerializeField] protected int _maxMagAmmo;
    public int MaxMagAmmo {
        get { return _maxMagAmmo; }
        set { _maxMagAmmo = value; }
    }

    [SerializeField] protected int _currentAmmo;
    public int CurrentAmmo {
        get { return _currentAmmo; }
        set { _currentAmmo = value; }
    }

    [SerializeField] protected int _maxAmmo;
    public int MaxAmmo {
        get { return _maxAmmo; }
        set { _maxAmmo = value; }
    }

    [SerializeField] protected float _range;
    public float Range {
        get { return _range; }
        set { _range = value; }
    }

    [SerializeField] protected float _fireRate;
    public float FireRate {
        get { return _fireRate; }
        set { _fireRate = value; }
    }

    [SerializeField] protected float _reloadTime;
    public float ReloadTime {
        get { return _reloadTime; }
        set { _reloadTime = value; }
    }

    [SerializeField] protected float _accuracyFacor;
    public float AccuracyFacor {
        get { return _accuracyFacor; }
        set { _accuracyFacor = value; }
    }

    [SerializeField] protected float _recoil;
    public float Recoil {
        get { return _recoil; }
        set { _recoil = value; }
    }

    public ParticleSystem Flash { get; set; }

    [SerializeField] private GameObject _hitEffect;
    public GameObject HitEffect {
        get { return _hitEffect; }
        set { _hitEffect = value; }
    }

    [SerializeField] private GameObject _smoke;
    public GameObject Smoke {
        get { return _smoke; }
        set { _smoke = value; }
    }


    private float _justFiredTimer = 0;
    private float _justReloadedTimer = 0;

    public enum WeaponState {
        idle,
        shooting,
        reloading
    };

    [SerializeField] private WeaponState _state;

    public WeaponState State {
        get { return _state; }
        set { _state = value; }
    }

    public enum FireMode {
        single,
        triple,
        continous
    };

    [SerializeField] private bool allowTriple;
    [SerializeField] private bool allowContinous;
    [SerializeField] private FireMode _defaultFireMode;
    [SerializeField] private FireMode _fireMode;

    public FireMode Mode {
        get { return _fireMode; }
        set { _fireMode = value; }
    }



    void Start() {
        Flash = transform.GetChild(0).GetChild(0).GetComponentInChildren<ParticleSystem>();
        _state = WeaponState.idle;
        _fireMode = _defaultFireMode;
    }

    private void Update() {
        if (_state.Equals(WeaponState.shooting)) {
            _justFiredTimer -= Time.deltaTime;
            if (_justFiredTimer < 0) {
                _justFiredTimer = 0;
                _state = WeaponState.idle;
            }
        }

        if (_state.Equals(WeaponState.reloading)) {
            _justReloadedTimer -= Time.deltaTime;
            if (_justReloadedTimer < 0) {
                _justReloadedTimer = 0;
                _state = WeaponState.idle;
                if (_currentAmmo > _maxMagAmmo) _currentMagAmmo = _maxMagAmmo;
                else _currentMagAmmo = _currentAmmo;
            }
        }
    }

    public void reload() {
        _justReloadedTimer = _reloadTime;
        _state = WeaponState.reloading;
    }


    public void shoot() {
        _currentAmmo--;
        _currentMagAmmo--;
        _justFiredTimer = _fireRate;
        _state = WeaponState.shooting;
    }

    public void changeFireMode() {
        if (_fireMode == FireMode.single) _fireMode = FireMode.triple;
        else if (_fireMode == FireMode.triple) _fireMode = FireMode.continous;
        else if (_fireMode == FireMode.continous) _fireMode = FireMode.single;

        if (!allowTriple && _fireMode == FireMode.triple) _fireMode = FireMode.continous;
        else if (!allowContinous && _fireMode == FireMode.continous) _fireMode = FireMode.single;
    }
    public void resetAmmo() {
        _currentAmmo = MaxAmmo;
        _currentMagAmmo = MaxMagAmmo;
    }
}
