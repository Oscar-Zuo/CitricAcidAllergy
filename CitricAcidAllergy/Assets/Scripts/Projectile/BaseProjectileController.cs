
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BaseProjectileController : MonoBehaviour
{
    private ProjectilePreset _preset;
    private BaseTurretController _launcher = null;
    private Rigidbody2D _rigidbody;

    private Vector2 _intitialVeolcity = Vector2.zero;


    public int Penetration { get => _preset.Penetration; protected set => _preset.Penetration = value; }
    public float DefaultDamage { get => _preset.Damage; protected set => _preset.Damage = value; }
    public BaseTurretController Launcher { get => _launcher; protected set => _launcher = value; }
    public float Speed { get => _preset.Speed; protected set => _preset.Speed = value; }
    public ProjectilePreset Preset { get => _preset; }

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    protected virtual void LateUpdate()
    {
        Move();
    }

    public virtual void Initialize(Transform pivotPoint, BaseTurretController turret, ProjectilePreset preset)
    {
        transform.SetPositionAndRotation(pivotPoint.position, pivotPoint.rotation);
        transform.localScale = Vector3.one;
        _launcher = turret;
        _preset = preset;

        ExpendSizeWithDamage();

        _intitialVeolcity = GameManager.Instance.Ship.ShipLinearVelocity;
    }

    protected virtual void ExpendSizeWithDamage()
    {
        transform.localScale *= 1 + (GetDamage() - 1) * (1 + Preset.sizeModifier);
    }

    protected virtual void Move()
    {
        Vector2 speed = Speed * transform.up;
        _rigidbody.linearVelocity = speed + _intitialVeolcity;
    }

    public virtual void Release()
    {
        if (!gameObject.activeSelf)
        {
            return;
        }
        GameManager.Instance.ProjectilePool.Release(this);
    }

    protected virtual void SetPreset(ProjectilePreset _newPreset)
    {
        _preset = _newPreset;
    }

    public virtual float GetDamage(BaseEnemyController enemy = null)
    {
        return DefaultDamage;
    }

    public void OnHitEnemy(BaseEnemyController enemy)
    {
        if (enemy == null || !gameObject.activeSelf)
        {
            return;
        }

        if (Penetration == -1) 
            return;

        Penetration--;

        if (Penetration <= 0)
        {
            Release();
        }
    }
}
