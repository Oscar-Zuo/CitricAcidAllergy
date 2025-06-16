using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[ExecuteInEditMode]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class ShipController : MonoBehaviour
{
    [SerializeField]
    private Vector2Int _size = new(5, 5);

    [SerializeField]
    private float _maxMovementSpeed = 1.0f;
    [SerializeField]
    private float _movementForce = 1.0f;

    private Vector2 _movingDirection = Vector2.zero;

    [SerializeField]
    private Transform _platformParent;
    [SerializeField]
    private GameObject _platformPrefab;

    [SerializeField]
    private MotherController _mother;

    private BoxCollider2D _boxCollider;
    private Rigidbody2D _rigidbody;

    [SerializeField]
    private PlatformController[][] _platforms;

    private Vector2 PlatfromSize => GameManager.Instance.GridSize;

    public Vector2 ShipLinearVelocity => _rigidbody.linearVelocity;

    public Vector2Int Size { get => _size; }
    public Transform PlatformParent { get => _platformParent; }

    protected virtual Vector2 GetPlatformWorldPositionByGrid(Vector2Int grid)
    {
        // Ship pivot start from buttom left
        return new Vector2(transform.position.x, transform.position.y) + new Vector2(grid.x * PlatfromSize.x, grid.y * PlatfromSize.y);
    }

    private void Awake()
    {
        InitializeBoxCollider();

        _rigidbody = GetComponent<Rigidbody2D>();

        InitializePlatforms();

        _mother.Initialize();
    }

    private void LateUpdate()
    {
        if (_movingDirection.y != 0)
        {
            _rigidbody.AddForceY(_movingDirection.y * _movementForce);
            _rigidbody.linearVelocityY = Mathf.Clamp(_rigidbody.linearVelocityY, -_maxMovementSpeed, _maxMovementSpeed);
        }

        var yLimitation = GameManager.Instance.MapYLimiation - new Vector2(0, GameManager.Instance.ShipSize.y);
        transform.position = new(transform.position.x, Mathf.Clamp(transform.position.y, yLimitation.x, yLimitation.y), transform.position.z);
    }

    private void InitializeBoxCollider()
    {
        _boxCollider = GetComponent<BoxCollider2D>();
        _boxCollider.offset = new Vector2(PlatfromSize.x * _size.x / 2.0f, PlatfromSize.y * _size.y / 2.0f);
        _boxCollider.size = new Vector2(PlatfromSize.x * _size.x, PlatfromSize.y * _size.y);
    }

    private void InitializePlatforms()
    {
        for (int i = 0; i < PlatformParent.childCount; ++i)
        {
            DestroyImmediate(PlatformParent.GetChild(i).gameObject);
        }

        _platforms = new PlatformController[_size.x][];
        for (int i = 0; i < _size.x; i++)
        {
            _platforms[i] = new PlatformController[_size.y];
            for (int j = 0; j < _size.y; j++)
            {
                var grid = new Vector2Int(i, j);

                var newPlatformObject = Instantiate(_platformPrefab, GetPlatformWorldPositionByGrid(grid), Quaternion.identity, _platformParent);
                if (!newPlatformObject.TryGetComponent<PlatformController>(out var platformController))
                {
                    Debug.LogWarning("PlatformController not found in Prefab!");
                    continue;
                }

                platformController.Initialize(grid);

                _platforms[i][j] = platformController;
            }
        }
    }

    public PlatformController GetPlatformControllerByGrid(Vector2Int grid)
    {
        if (grid.x < 0 || grid.y < 0 || grid.x >= _size.x || grid.y >= _size.y)
        {
            Debug.LogWarning("Grid out of bound!");
            return null;
        }

        return _platforms[grid.x][grid.y];
    }

    public Vector2Int GetGridByWorldPosition(Vector2 worldPosition)
    {
        Vector2 diff = worldPosition - new Vector2(transform.position.x, transform.position.y);
        if (diff.x < 0 || diff.y < 0 || diff.x > _size.x * PlatfromSize.x || diff.y > _size.y * PlatfromSize.y)
        {
            Debug.LogWarning("Invaild worldPostion!");
            return -Vector2Int.one;
        }

        int x = Mathf.FloorToInt(diff.x / PlatfromSize.x);
        int y = Mathf.FloorToInt(diff.y / PlatfromSize.y);

        return new Vector2Int(x, y);
    }

    public bool TryGetMouseOnShipGrid(out Vector2 gridWorldPosition, out Vector2Int pointingGrid)
    {
        Vector2 screenPosition = Input.mousePosition;
        Vector2 worldPoint = Camera.main.ScreenToWorldPoint(screenPosition);

        RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero, 100, LayerMask.GetMask("Ship"));
        if (hit.collider == null)
        {
            gridWorldPosition = worldPoint;
            pointingGrid = -Vector2Int.one;
            return false;
        }

        pointingGrid = GetGridByWorldPosition(worldPoint);
        gridWorldPosition = GetPlatformControllerByGrid(pointingGrid).Anchor.position;
        return true;
    }


    public virtual bool CanPlaceTurretOnGrid(Vector2Int grid, TurretPreset turretPreset)
    {
        PlatformController platform = GetPlatformControllerByGrid(grid);
        if (platform != null)
            return platform.Turret == null;
        return false;
    }

    public bool OnReceiveReinforcementPreset(ReinforcementPreset reinforcementPreset, Vector2Int dropGrid)
    {
        if (reinforcementPreset == null)
        {
            return false;
        }

        foreach (var pair in reinforcementPreset.Turrets)
        {
            if (!CanPlaceTurretOnGrid(pair.Key + dropGrid, pair.Value))
            {
                return false;
            }
        }

        foreach (var pair in reinforcementPreset.Turrets)
        {
            PlatformController platform = GetPlatformControllerByGrid(pair.Key + dropGrid);
            platform.SpawnTurret(pair.Value);
        }

        return true;
    }

    public bool OnReceivePluginPreset(BaseTurretPluginPreset pluginPreset, Vector2Int dropGrid)
    {
        if (pluginPreset == null)
        {
            return false;
        }

        var platform = GetPlatformControllerByGrid(dropGrid);
        if (platform == null || platform.Turret == null)
        {
            return false;
        }

        return platform.Turret.AddPlugin(pluginPreset);
    }

    public void OnMoveShip(InputValue rawInputValue)
    {
        _movingDirection = rawInputValue.Get<Vector2>();
    }

    private void StandarlizeStartEndGrid(ref Vector2Int startGrid, ref Vector2Int endGrid)
    {
        int minX = Mathf.Min(startGrid.x, endGrid.x);
        int minY = Mathf.Min(startGrid.y, endGrid.y);
        int maxX = Mathf.Max(startGrid.x, endGrid.x);
        int maxY = Mathf.Max(startGrid.y, endGrid.y);

        // from grid is alway buttom left
        startGrid = new Vector2Int(minX, minY);
        endGrid = new Vector2Int(maxX, maxY);
    }

    public ReinforcementPreset GenerateReinforcementPreset(Vector2Int startGrid, Vector2Int endGrid)
    {
        StandarlizeStartEndGrid(ref startGrid, ref endGrid);

        List<Tuple<Vector2Int, TurretPreset>> turretGridPresets = new();

        var minX = int.MaxValue;
        var minY = int.MaxValue;

        for (int i = 0; i <= endGrid.x - startGrid.x; i++)
        {
            int x = startGrid.x + i;
            for (int j = 0; j <= endGrid.y - startGrid.y; j++)
            {
                int y = startGrid.y + j;
                var grid = new Vector2Int(x, y);
                var platform = GetPlatformControllerByGrid(grid);

                if (platform!=null && platform.IsOccupied)
                {
                    var turretPreset = platform.Turret.GenerateCurrentPreset();

                    minX = Mathf.Min(minX, grid.x);
                    minY = Mathf.Min(minY, grid.y);
                    turretGridPresets.Add(new(grid, turretPreset));
                }
            }
        }

        if (turretGridPresets.Count == 0)
            return null;

        for (int i = 0; i< turretGridPresets.Count;++i)
        {
            turretGridPresets[i] = new(turretGridPresets[i].Item1 - new Vector2Int(minX, minY), turretGridPresets[i].Item2);
        }

        return new ReinforcementPreset(turretGridPresets.ToDictionary(t => t.Item1, t => t.Item2), source: ReinforcementSourceType.Ship);
    }

    public void ClearTurretOnGrids(Vector2Int startGrid, Vector2Int endGrid)
    {
        StandarlizeStartEndGrid(ref startGrid, ref endGrid);

        for (int i = startGrid.x; i <= endGrid.x; ++i)
        {
            for (int j = startGrid.y; j <= endGrid.y; ++j)
            {
                var platform = GetPlatformControllerByGrid(new Vector2Int(i, j));
                if (platform == null || !platform.IsOccupied)
                {
                    continue;
                }

                platform.RemoveTurret();
            }
        }
    }
}
