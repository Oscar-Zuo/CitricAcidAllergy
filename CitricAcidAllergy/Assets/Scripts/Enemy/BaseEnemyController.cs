using UnityEngine;
using UnityEngine.Rendering.LookDev;

[RequireComponent(typeof(Collider2D))]
public class BaseEnemyController : MonoBehaviour
{
    [SerializeField]
    private float _baseMaxHealth = 5;
    private float _currentHealth;

    [SerializeField]
    private float _baseSpeed = 1.0f;
    private float _speed = 1.0f;

    private Vector2 _forwardDirection = Vector2.right;

    private int _row;
    private EnemyPool _poolBelongTo;
    private Collider2D _collider;
    private bool _giveRewardOnDeath = false;

    [SerializeField]
    private Animator _animator;
    private readonly int _onHitTriggerHash = Animator.StringToHash("OnHit");

    public float BaseMaxHealth { get => _baseMaxHealth; protected set => _baseMaxHealth = value; }
    public float Speed { get => _speed; protected set => _speed = value; }
    public Vector2 ForwardDirection { get => _forwardDirection; protected set => _forwardDirection = value; }
    public int Row { get => _row; protected set => _row = value; }
    public EnemyPool PoolBelongTo { get => _poolBelongTo; }
    public float CurrentHealth 
    { 
        get => _currentHealth;
        protected set
        {
            _currentHealth = Mathf.Clamp(value, 0, _baseMaxHealth);
        }
    }

    protected virtual void Update()
    {
        // The enemy will only have very basic AI
        // It is OK to use basic translate

        Vector2 _delta = Time.deltaTime * Speed * ForwardDirection;

        transform.Translate(_delta);
    }

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
    }

    public virtual void Initialize(Vector2 position, Quaternion rotation, EnemyPool _pool, float level, bool generateReward = false)
    {
        transform.SetPositionAndRotation(position, rotation);
        _poolBelongTo = _pool;
        ApplyLevelModifier(level);
        _giveRewardOnDeath = generateReward;
    }

    protected virtual void ApplyLevelModifier(float level)
    {
        // Should make this a scriptable object. Running out of time
        transform.localScale = Vector3.one + Vector3.one * (level * 0.01f);
        CurrentHealth = BaseMaxHealth + BaseMaxHealth * (level * 0.25f);
        Speed = _baseSpeed + _baseSpeed * (level * 0.01f);
    }

    public void Release()
    {
        PoolBelongTo.Release(this);
    }
    public virtual void OnRelease()
    {

    }

    public virtual void OnDead()
    {
        _poolBelongTo.Release(this);
        SpawnDeathAnimation();
        if (_giveRewardOnDeath)
            GameManager.Instance.GiveReward();
    }

    protected virtual void OnHit(BaseProjectileController projectile)
    {
        CurrentHealth -= projectile.GetDamage(this);
        _animator.SetTrigger(_onHitTriggerHash);

        if (CurrentHealth <= 0)
        {
            OnDead();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Projectile") && collision.TryGetComponent<BaseProjectileController>(out var projectileController))
        {
            OnHitProjectile(projectileController);
        }
        else if (collision.CompareTag("Turret") && collision.TryGetComponent<BaseTurretController>(out var turretController))
        {
            OnHitTurret(turretController);
        }
        else if (collision.CompareTag("Mother") && collision.TryGetComponent<MotherController>(out var motherController))
        {
            OnHitMother(motherController);
        }
    }

    private void OnHitProjectile(BaseProjectileController projectile)
    {
        projectile.OnHitEnemy(this);
        OnHit(projectile);
    }

    private void OnHitTurret(BaseTurretController turretController)
    {
        OnDead();
        turretController.OnHitByEnemy();
    }

    private void OnHitMother(MotherController motherController)
    {
        OnDead();
        motherController.OnHitByEnemy();
    }

    private void SpawnDeathAnimation(GameObject effectPrefab = null)
    {
        if (effectPrefab == null)
        {
            effectPrefab = GameManager.Instance.DefaultDeathEffectPrefab;
        }

        Instantiate(effectPrefab, transform.position, Quaternion.identity);
    }
}
