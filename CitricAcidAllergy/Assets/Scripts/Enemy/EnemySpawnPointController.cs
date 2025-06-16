using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Events;

public class EnemySpawnPointController : MonoBehaviour
{
    [SerializeField]
    private Transform _targetTransfrom;
    [SerializeField]
    private Vector3 _defaultSpawnRotation;
    [SerializeField]
    private int _rows = 5;

    private EnemyPool _enemyPool;

    private Coroutine _spawnHordeCoroutine;

    // Controller, row index
    public static UnityEvent<BaseEnemyController> onEnemySpawn = new();
    public static UnityEvent<BaseEnemyController> onEnemyDestroy = new();

    public Transform TargetTransfrom { get => _targetTransfrom; }
    public EnemyPool EnemyPool { get => _enemyPool; set => _enemyPool = value; }
    public int Rows { get => _rows; }

    private void Awake()
    {
        _enemyPool = new EnemyPool(this);
    }

    #region Functions for Enemy Pool
    public virtual BaseEnemyController SpawnEnemy()
    {
        var newEnemyObject = Instantiate(GetNextSpawnPrefab(), _targetTransfrom);
        if (!newEnemyObject.TryGetComponent<BaseEnemyController>(out var controller))
        {
            Debug.LogWarning("No enemy controller found in the prefab!");
            return null;
        }
        return controller;
    }

    public virtual void DestroyEnemy(BaseEnemyController enemyController)
    {
        DisableEnemy(enemyController);
        Destroy(enemyController.gameObject);
    }
    #endregion

    private void Start()
    {
        _spawnHordeCoroutine = StartCoroutine(SpawnHorde());
    }

    private IEnumerator SpawnHorde()
    {
        while (true)
        {
            GetEnemysBasedOnDifficulty(GameManager.Instance.GetDifficultyLevelFromTime());
            yield return new WaitForSeconds(GameManager.Instance.SpawnHordeInterval);
        }
    }

    private IEnumerator SpawnEnemiesInRow(List<float> enemyLevels)
    {
        int index = 0;
        BaseEnemyController enemyController = null;
        for (int i = 0; i < enemyLevels.Count - 1; i++)
        {
            enemyController = GetEnemy();
            enemyController.Initialize(GetNextSpawnPosition(index), GetNextSpawnRotation(), EnemyPool, enemyLevels[i]);

            index++;
            if (index % Rows == 0)
            {
                index = 0;
                yield return new WaitForSeconds(GameManager.Instance.SpawnEnemyInRowInterval);
            }
        }

        enemyController = GetEnemy();
        bool generateReward = Random.value < GameManager.Instance.GenerateRewardChange;
        enemyController.Initialize(GetNextSpawnPosition(index), GetNextSpawnRotation(), EnemyPool, enemyLevels[enemyLevels.Count - 1], generateReward);

    }

    public virtual void GetEnemysBasedOnDifficulty(float difficulty)
    {
        var enemyLevels = GenerateRandomFloats(difficulty);
        StartCoroutine(SpawnEnemiesInRow(enemyLevels));
    }
    public virtual BaseEnemyController GetEnemy()
    {
        var newEnemy = EnemyPool.Get();
        onEnemySpawn?.Invoke(newEnemy);
        return newEnemy;
    }


    public virtual void ReleaseEnemy(BaseEnemyController enemyController)
    {
        enemyController.OnRelease();
        onEnemyDestroy?.Invoke(enemyController);
        EnemyPool.Release(enemyController);
    }

    protected virtual void DisableEnemy(BaseEnemyController enemyController)
    {
        onEnemyDestroy?.Invoke(enemyController);
    }

    protected virtual GameObject GetNextSpawnPrefab()
    {
        return GameManager.Instance.DefaultEnemyPrefab;
    }

    protected virtual Vector2 GetNextSpawnPosition(int row)
    {
        return _targetTransfrom.position + new Vector3(0, GameManager.Instance.GridSize.y * row, 0);
    }

    protected virtual Quaternion GetNextSpawnRotation()
    {
        return Quaternion.Euler(_defaultSpawnRotation);
    }

    private static List<float> GenerateRandomFloats(float total)
    {
        int maxCount = Mathf.FloorToInt(total);
        if (maxCount <= 0)
            return null;

        int count = Random.Range(1, maxCount + 1);

        float[] raw = new float[count];
        float rawSum = 0f;
        for (int i = 0; i < count; i++)
        {
            float u = Mathf.Clamp01(Random.value);
            raw[i] = -Mathf.Log(u + Mathf.Epsilon);
            rawSum += raw[i];
        }

        List<float> result = new List<float>();
        float stretch = total - count;
        for (int i = 0; i < count; i++)
        {
            float xi = 1f + (raw[i] / rawSum) * stretch;
            result.Add(xi);
        }

        return result;
    }
}
