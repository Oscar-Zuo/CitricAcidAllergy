using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class KillZoneController : MonoBehaviour
{
    public bool killProjectile = true;
    public bool killEnemy = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (killEnemy && collision.CompareTag("Enemy"))
        {
            var enemyController = collision.GetComponent<BaseEnemyController>();
            enemyController.Release();
        }

        if (killProjectile && collision.CompareTag("Projectile"))
        {
            var projectileController = collision.GetComponent<BaseProjectileController>();
            projectileController.Release();
        }
    }
}
