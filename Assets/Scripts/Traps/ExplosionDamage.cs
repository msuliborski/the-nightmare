using System.Collections.Generic;
using UnityEngine;

// Shared server-side damage for explosion triggers (grenades, barrels). Enemies have separate
// head/body/legs colliders and players several colliders too, so without de-duplication one
// explosion hit the same target up to three times.
public class ExplosionDamage
{
    public const float PLAYER_DAMAGE = 45f;
    private readonly HashSet<Object> _alreadyHit = new HashSet<Object>();

    public void Apply(Collider other, float enemyDamage)
    {
        if (other.CompareTag("EnemyLegs") || other.CompareTag("EnemyHead") || other.CompareTag("EnemyBody"))
        {
            EnemyControllerServer enemy = other.GetComponentInParent<EnemyControllerServer>();
            if (enemy != null && _alreadyHit.Add(enemy)) enemy.TakeDamage(enemyDamage);
        }
        else if (other.CompareTag("Player"))
        {
            PlayerManager player = other.GetComponentInParent<PlayerManager>();
            if (player != null && _alreadyHit.Add(player)) player.TakeDamage(PLAYER_DAMAGE);
        }
        else if (other.CompareTag("Barrel"))
        {
            Barrel barrel = other.GetComponent<Barrel>();
            if (barrel != null) barrel.Explode();
        }
    }
}
