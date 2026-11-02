using UnityEngine;

// Was a NetworkBehaviour although it never used networking; the explosion only acts on the server.
public class GrenadeCollider : MonoBehaviour
{
    public bool server = false;
    private float _damage;
    private readonly ExplosionDamage _explosion = new ExplosionDamage();

    void Start()
    {
        _damage = transform.GetComponentInParent<Grenade>()._damage;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (server) _explosion.Apply(other, _damage);
    }
}
