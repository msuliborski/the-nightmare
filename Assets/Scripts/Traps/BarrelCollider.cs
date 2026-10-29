using UnityEngine;

public class BarrelCollider : MonoBehaviour
{
    public bool server = false;
    private float _damage;
    private readonly ExplosionDamage _explosion = new ExplosionDamage();

    void Start()
    {
        _damage = transform.GetComponentInParent<Barrel>().damage;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (server) _explosion.Apply(other, _damage);
    }
}
