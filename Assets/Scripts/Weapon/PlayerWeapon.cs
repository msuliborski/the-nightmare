using UnityEngine;
using UnityEngine.Networking;

public class PlayerWeapon : NetworkBehaviour
{
    [SerializeField] private string _name;
    public string Name { get { return _name; } set { _name = value; } }
    [SerializeField] private int _damage;
    public int Damage { get { return _damage; } set { _damage = value; } }
    [SerializeField] private float _range;
    public float Range { get { return _range; } set { _range = value; } }

    public ParticleSystem Flash { get; set; }
    public ParticleSystem Smoke { get; set; }
    public GameObject HitEffect { get; set; }

    void Start()
    {
        Flash = transform.GetChild(0).GetComponentInChildren<ParticleSystem>();
        Smoke = transform.GetChild(0).GetChild(0).GetComponentInChildren<ParticleSystem>();
    }
}
