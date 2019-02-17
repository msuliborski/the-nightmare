using UnityEngine;
using UnityEngine.Networking;

public class PlayerWeapon : NetworkBehaviour
{
    [SerializeField] private string _name;
    public string Name { get { return _name; } set { _name = value; } }
    [SerializeField] private float _damage;
    public float Damage { get { return _damage; } set { _damage = value; } }
    [SerializeField] private float _range;
    public float Range { get { return _range; } set { _range = value; } }
}
