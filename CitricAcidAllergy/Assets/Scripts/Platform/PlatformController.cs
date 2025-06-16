using UnityEngine;

public class PlatformController : MonoBehaviour
{
    [SerializeField]
    private Transform _anchor;
    private BaseTurretController _turret;

    private Vector2Int _grid;

    protected ShipController Ship => GameManager.Instance.Ship;

    public BaseTurretController Turret { get => _turret; protected set => _turret = value; }
    public bool IsOccupied => _turret != null;
    public int Row { get => _grid.x; }

    public Transform Anchor { get => _anchor; }

    public virtual void Initialize(Vector2Int grid)
    {
        _grid = grid;
    }

    private void Start()
    {

    }

    public virtual void SpawnTurret(TurretPreset preset, GameObject turretPrefab = null)
    {
        if (turretPrefab == null)
        {
            turretPrefab = GameManager.Instance.DefaultTurretPrefab;
        }

        var turretObject = Instantiate(turretPrefab, Anchor);
        if (!turretObject.TryGetComponent<BaseTurretController>(out var turretController))
        {
            Debug.LogWarning("TurretController not found in Prefab!");
            return;
        }

        turretController.Initialize(preset);
        Turret = turretController;
    }

    public virtual void RemoveTurret()
    {
        if (!IsOccupied)
        {
            return;
        }

        Turret.SelfDestroy();
        Turret = null;
    }
}
