using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class DraggingManager : MonoBehaviour
{
    static DraggingManager _instance;

    private ReinforcementPreset _draggingReinforcementPreset;
    private BaseTurretPluginPreset _draggingPluginPreset;

    [SerializeField]
    private PreviewGroupController _previewGroupController;

    [SerializeField]
    private SelectionBoxController _selectionBoxController;

    [SerializeField]
    private GraphicRaycaster _canvasRaycaster;

    private Vector2? _startDraggingPosition;

    public bool IsPreviewShowing => _previewGroupController.gameObject.activeSelf;

    public static DraggingManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindAnyObjectByType<DraggingManager>();
            }

            return _instance;
        }
    }

    public void DragReinforcementPreset(ReinforcementPreset draggingPreset, PointerEventData eventData)
    {
        _draggingReinforcementPreset = draggingPreset;

        _previewGroupController.ShowPreview(draggingPreset, grid =>
        {
            var platform = GameManager.Instance.Ship.GetPlatformControllerByGrid(grid);
            if (platform == null)
            {
                return false;
            }
            else
            {
                return !platform.IsOccupied;
            }
        });
        _previewGroupController.gameObject.SetActive(true);

        GameManager.Instance.PauseGame();
    }

    public void OnDraggingReinforcementPreset(PointerEventData eventData)
    {
    }

    public bool DropReinforcementPreset(PointerEventData eventData)
    {
        GameManager.Instance.UnpauseGame();
        _previewGroupController.gameObject.SetActive(false);

        if (_draggingReinforcementPreset == null)
        {
            return false;
        }

        if (!GameManager.Instance.Ship.TryGetMouseOnShipGrid(out var _, out Vector2Int droppingGrid))
        {
            return false;
        }

        var result = GameManager.Instance.Ship.OnReceiveReinforcementPreset(_draggingReinforcementPreset, droppingGrid);
        _draggingReinforcementPreset = null;
        return result;
    }

    public void DragPluginPreset(BaseTurretPluginPreset draggingPreset, PointerEventData eventData)
    {
        _draggingPluginPreset = draggingPreset;

        _previewGroupController.ShowPreview(_draggingPluginPreset, grid =>
        {
            var platform = GameManager.Instance.Ship.GetPlatformControllerByGrid(grid);
            if (platform == null || platform.Turret == null)
            {
                return false;
            }
            else
            {
                return platform.Turret.CanPlacePlugin(draggingPreset);
            }
        });
        _previewGroupController.gameObject.SetActive(true);

        GameManager.Instance.PauseGame();
    }

    public void OnDraggingPluginPreset(PointerEventData eventData)
    {
    }

    public bool DropPluginPreset(PointerEventData eventData)
    {
        GameManager.Instance.UnpauseGame();

        _previewGroupController.gameObject.SetActive(false);

        if (_draggingPluginPreset == null)
        {
            return false;
        }

        if (!GameManager.Instance.Ship.TryGetMouseOnShipGrid(out var _, out Vector2Int droppingGrid))
        {
            return false;
        }

        var result = GameManager.Instance.Ship.OnReceivePluginPreset(_draggingPluginPreset, droppingGrid);
        _draggingPluginPreset = null;
        return result;
    }

    private Vector2Int GetOutBoundGrid(Vector2 worldPoint)
    {
        Vector2 position = worldPoint;

        if (GameManager.Instance.Ship.transform.position.x > worldPoint.x)
        {
            position.x = GameManager.Instance.Ship.transform.position.x + 0.01f;
        }
        else if (GameManager.Instance.Ship.transform.position.x + GameManager.Instance.ShipSize.x < worldPoint.x)
        {
            position.x = GameManager.Instance.Ship.transform.position.x + GameManager.Instance.ShipSize.x - 0.01f;
        }

        if (GameManager.Instance.Ship.transform.position.y > worldPoint.y)
        {
            position.y = GameManager.Instance.Ship.transform.position.y + 0.01f;
        }
        else if (GameManager.Instance.Ship.transform.position.y + GameManager.Instance.ShipSize.y < worldPoint.y)
        {
            position.y = GameManager.Instance.Ship.transform.position.y + GameManager.Instance.ShipSize.y - 0.01f;
        }
        return GameManager.Instance.Ship.GetGridByWorldPosition(position);
    }

    private Rect GetRectFromOppositePoints(Vector2 pointA, Vector2 pointB)
    {
        Vector2 min = Vector2.Min(pointA, pointB);
        Vector2 size = Vector2.Max(pointA, pointB) - min;

        return new Rect(min, size);
    }

    private bool IsPointerOverUI()
    {
        var eventData = new PointerEventData(EventSystem.current)
        {
            position = Pointer.current.position.ReadValue()
        };

        var results = new List<RaycastResult>();
        _canvasRaycaster.Raycast(eventData, results);

        var resultCount = results.Count;

        foreach (var result in results)
        {
            if (result.gameObject.CompareTag("SelectionBox"))
            {
                resultCount--;
            }
        }

        return resultCount > 0;
    }

    public void OnEditingShipMouseDown(InputAction.CallbackContext context)
    {
        if (IsPointerOverUI())
        {
            _startDraggingPosition = null;
            return;
        }
            
        if (context.started)
        {
            StartSelection();
        }
        else if (context.canceled)
        {
            EndSelection();
        }
    }

    private void StartSelection()
    {
        _selectionBoxController.StartSelection();

        Vector2 screenPosition = Input.mousePosition;
        _startDraggingPosition = Camera.main.ScreenToWorldPoint(screenPosition);
    }

    private void EndSelection()
    {
        _selectionBoxController.EndSelection();

        if (_startDraggingPosition != null)
        {
            if (!GameManager.Instance.Ship.TryGetMouseOnShipGrid(out var endWorldPosition, out var endGrid))
            {
                Rect shipRect = GetRectFromOppositePoints(GameManager.Instance.Ship.transform.position,
                    new Vector2(GameManager.Instance.Ship.transform.position.x, GameManager.Instance.Ship.transform.position.y) + GameManager.Instance.ShipSize);

                Rect dragRect = GetRectFromOppositePoints(_startDraggingPosition.Value, endWorldPosition);

                if (!shipRect.Overlaps(dragRect))
                {
                    return;
                }

                endGrid = GetOutBoundGrid(endWorldPosition);
            }

            var startGrid = GetOutBoundGrid(_startDraggingPosition.Value);
            var newPreset = GameManager.Instance.Ship.GenerateReinforcementPreset(startGrid, endGrid);

            if (newPreset == null)
            {
                return;
            }

            GameManager.Instance.NotificationPanel.ShowMessage("Send out helps?",
                "Do you wish to send out selected soldiers to other players? Doing this will increase your contribution points.",
                () =>
                {
                    GameManager.Instance.SellReinforcement(newPreset);
                    GameManager.Instance.Ship.ClearTurretOnGrids(startGrid, endGrid);
                }, () => { });
        }

        _startDraggingPosition = null;
    }
}
