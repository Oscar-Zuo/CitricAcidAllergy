using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class PreviewGroupController : MonoBehaviour
{
    [SerializeField]
    private Transform _targetTransform;
    [SerializeField]
    private GameObject _previewPrefab;

    private Dictionary<Vector2Int, PreviewController> _spawnedPreview = new();

    public delegate bool ValidationCheckDelegate(Vector2Int currentGrid);
    private ValidationCheckDelegate _validationCheck = null;

    private ShipController ship => GameManager.Instance.Ship;

    private void Update()
    {
        if (ship.TryGetMouseOnShipGrid(out var gridWorldPosition, out var pointingGrid))
        {
            ClipToPosition(gridWorldPosition);
            UpdateValibility(pointingGrid);
        }
        else 
        {
            ClipToMouse();
            SetPreviewToDefault();
        }
    }

    private void OnDisable()
    {
        ClearOldPreview();
    }

    public void ShowPreview(ReinforcementPreset reinforcementPreset, ValidationCheckDelegate validationCheck)
    {
        ClearOldPreview();
        IntializePreview(reinforcementPreset, validationCheck);
    }

    public void ShowPreview(BaseTurretPluginPreset pluginPreset, ValidationCheckDelegate validationCheck)
    {
        ClearOldPreview();
        IntializePreview(pluginPreset, validationCheck);
    }

    private void ClearOldPreview()
    {
        foreach (var oldPreview in _spawnedPreview.Values)
        {
            Destroy(oldPreview.gameObject);
        }
        _spawnedPreview.Clear();
    }

    private void IntializePreview(ReinforcementPreset reinforcementPreset, ValidationCheckDelegate validationCheck)
    {
        _validationCheck = validationCheck;

        foreach (var pair in reinforcementPreset.Turrets)
        {
            var newPreviewObject = Instantiate(_previewPrefab, _targetTransform);
            if (!newPreviewObject.TryGetComponent(out PreviewController previewController))
            {
                Debug.LogWarning("PreviewTurretController not found in preview object!");
                return;
            }

            previewController.SetSprite(pair.Value.previewSprite);
            previewController.SwitchToDefault();

            Vector2 worldPosition = new(_targetTransform.transform.position.x, _targetTransform.transform.position.y);
            Vector2 offset = new(GameManager.Instance.GridSize.x * pair.Key.x, GameManager.Instance.GridSize.y * pair.Key.y);

            newPreviewObject.transform.position = worldPosition + offset;

            _spawnedPreview.Add(pair.Key, previewController);
        }
    }

    private void IntializePreview(BaseTurretPluginPreset pluginPreset, ValidationCheckDelegate validationCheck)
    {
        _validationCheck = validationCheck;

        var newPreviewObject = Instantiate(_previewPrefab, _targetTransform);
        if (!newPreviewObject.TryGetComponent(out PreviewController previewController))
        {
            Debug.LogWarning("PreviewTurretController not found in preview object!");
            return;
        }

        previewController.SetSprite(pluginPreset.previewSprite);
        previewController.SwitchToDefault();

        Vector2 worldPosition = new(_targetTransform.transform.position.x, _targetTransform.transform.position.y);

        newPreviewObject.transform.position = worldPosition;

        _spawnedPreview.Add(new(0, 0), previewController);
    }

    private void ClipToMouse()
    {
        Vector2 screenPosition = Input.mousePosition;
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);

        transform.position = worldPosition;
    }

    private void ClipToPosition(Vector2 position)
    {
        transform.position = position;
    }

    private void UpdateValibility(Vector2Int pointingGrid)
    {
        foreach (var pair in _spawnedPreview)
        {
            bool valibility = _validationCheck(pointingGrid + pair.Key);
                
            if (valibility)
            {
                pair.Value.SwitchToValid();
            }
            else
            {
                pair.Value.SwitchToInvalid();
            }
        }
    }    

    private void SetPreviewToDefault()
    {
        foreach (var preview in _spawnedPreview.Values)
        {
            preview.SwitchToDefault();
        }
    }
}
