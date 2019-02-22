using UnityEngine;
using UnityEngine.Networking;

public class PlayerWeapon : NetworkBehaviour
{
    [SerializeField] 
    protected string _name; 
    public string Name { get { return _name; } set { _name = value; } }
    
    [SerializeField] 
    protected  int _damage;
    public int Damage { get { return _damage; } set { _damage = value; } }
    
    [SerializeField] 
    protected  int _bulletsInMag;
    public int BulletsInMag { get { return _bulletsInMag; } set { _bulletsInMag = value; } }
    
    [SerializeField] 
    protected  int _magSize;
    public int MagSize { get { return _magSize; } set { _magSize = value; } }
    
    [SerializeField] 
    protected  int _ammo;
    public int Ammo { get { return _ammo; } set { _ammo = value; } }
     
    [SerializeField] 
    protected  int _maxAmmo;
    public int MaxAmmo { get { return _maxAmmo; } set { _maxAmmo = value; } }
    
    [SerializeField] 
    protected  float _range;
    public float Range { get { return _range; } set { _range = value; } }
    
    [SerializeField] 
    protected  float _fireRate;
    public float FireRate { get { return _fireRate; } set { _fireRate = value; } }
    
    [SerializeField] 
    protected  float _reloadTime;
    public float ReloadTime { get { return _reloadTime; } set { _reloadTime = value; } }
    
    [SerializeField] 
    protected  float _accuracyFacor;
    public float AccuracyFacor { get { return _accuracyFacor; } set { _accuracyFacor = value; } }
    
    public ParticleSystem Flash { get; set; }
    
    [SerializeField] 
    private GameObject _hitEffect;
    public GameObject HitEffect { get { return _hitEffect; } set { _hitEffect = value; } }
    
    [SerializeField] 
    private GameObject _smoke;
    public GameObject Smoke { get { return _smoke; } set { _smoke = value; } }


    private float _justFiredTimer = 0;
    private float _justReloadedTimer = 0;
    
    protected enum State {idle, shooting, reloading};
    protected State state;
    
    
    void Start()
    {
        Flash = transform.GetChild(0).GetChild(0).GetComponentInChildren<ParticleSystem>();
        state = State.idle;
    }

    private void Update()
    {
        
    }

    public void reload()
    {
    }


    public void fire()
    {
        _justFiredTimer = _fireRate;
        state = State.reloading;


    }


}
