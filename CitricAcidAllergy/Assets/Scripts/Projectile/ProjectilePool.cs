using System;
using UnityEngine;
using UnityEngine.Pool;

public class ProjectilePool
{
    private const int _maxProjectile = 1024;
    private ObjectPool<BaseProjectileController> _pool;

    private GameObject _nextSpawningProjectilePrefab;

    public ObjectPool<BaseProjectileController> Pool { get => _pool; }

    public ProjectilePool()
    {
        _pool = new ObjectPool<BaseProjectileController>(SpawnProjectile, TakeProjectile, ReleaseProjectile, DestroyProjectile, true, _maxProjectile);
    }

    public BaseProjectileController Get(GameObject projectilePrefab)
    {
        _nextSpawningProjectilePrefab = projectilePrefab;
        return _pool.Get();
    }

    public void Release(BaseProjectileController projectileController)
    {
        _pool.Release(projectileController);
    }

    private void DestroyProjectile(BaseProjectileController controller)
    {
        GameObject.Destroy(controller.gameObject);
    }

    private void ReleaseProjectile(BaseProjectileController controller)
    {
        controller.gameObject.SetActive(false);
    }

    private void TakeProjectile(BaseProjectileController controller)
    {
        controller.gameObject.SetActive(true);
    }

    private BaseProjectileController SpawnProjectile()
    {
        var projectileObject = GameObject.Instantiate(_nextSpawningProjectilePrefab);

        if (!projectileObject.TryGetComponent<BaseProjectileController>(out var controller))
        {
            Debug.LogWarning("No projectile controller found in the prefab!");
            return null;
        }

        return controller;
    }
}
