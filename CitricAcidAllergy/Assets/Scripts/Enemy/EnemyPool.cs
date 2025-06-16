using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

public class EnemyPool
{
    private readonly EnemySpawnPointController _bindedSpwanPoint;

    private const int _maxEnemy = 128;
    private ObjectPool<BaseEnemyController> _pool;

    public EnemyPool(EnemySpawnPointController i_bindedSpwanPoint)
    {
        _bindedSpwanPoint = i_bindedSpwanPoint;

        _pool = new ObjectPool<BaseEnemyController>(SpawnEnemy, TakeEnemy, ReleaseEnemy, DestroyEnemy, true, _maxEnemy);
    }

    public BaseEnemyController Get()
    {
        return _pool.Get();
    }

    public void Release(BaseEnemyController controller)
    {
        _pool.Release(controller);
    }

    private BaseEnemyController SpawnEnemy()
    {
        return _bindedSpwanPoint.SpawnEnemy();
    }

    private void TakeEnemy(BaseEnemyController enemyController)
    {
        enemyController.gameObject.SetActive(true);
    }

    private void ReleaseEnemy(BaseEnemyController enemyController)
    {
        enemyController.gameObject.SetActive(false);
    }

    private void DestroyEnemy(BaseEnemyController enemyController) 
    {
        _bindedSpwanPoint.DestroyEnemy(enemyController);
    }
}
