using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    public int damage = 1;
    public int dirX = 0;   // -1 left, +1 right
    public int dirY = 0;   // -1 down, +1 up}


    bool hasHit; // prevents multiple hits per attack instance

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;

        if (!other.CompareTag("Ennemy"))
            return;

        // Enemy collider might be on a child, so use InParent
        var enemy = other.GetComponentInParent<Ennemy>();
        if (enemy == null) return;

        // Apply damage respecting your cooldown system
        enemy.health = ApplyEnemyDamage(enemy, damage);

        // If we want knockback direction, we can store it on enemy
        // (depends on how the Ennemy script reads punch2Position currently)
        // Example: enemy.lastHitDirX = dirX; enemy.lastHitDirY = dirY;

        hasHit = true;
    }

    int ApplyEnemyDamage(Ennemy enemy, int dmg)
    {
        if (enemy.damageDecreasingCoolDown == 0)
        {
            enemy.damageDecreasingCoolDown = enemy.damageCoolDown;
            return enemy.health - (dmg - enemy.armor);
        }
        return enemy.health;
    }

}