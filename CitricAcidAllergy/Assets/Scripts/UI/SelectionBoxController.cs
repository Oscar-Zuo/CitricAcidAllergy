using UnityEngine;
using UnityEngine.UI;

public class SelectionBoxController : MonoBehaviour
{
    [SerializeField]
    private RectTransform _rect;
    [SerializeField]
    private Canvas _canvas;
    private Vector2 _startPos;
    private bool _isSelecting = false;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
    }

    public void StartSelection()
    {
        _isSelecting = true;
        _startPos = Input.mousePosition;
        _rect.gameObject.SetActive(true);
        UpdateSelectionBox(_startPos, _startPos); // zero size at start
    }

    void Update()
    {
        if (!_isSelecting) return;

        Vector2 currentMousePos = Input.mousePosition;
        UpdateSelectionBox(_startPos, currentMousePos);
    }

    public void EndSelection()
    {
        _isSelecting = false;
        _rect.gameObject.SetActive(false);
    }

    void UpdateSelectionBox(Vector2 screenStart, Vector2 screenEnd)
    {
        RectTransform _canvasRect = _canvas.transform as RectTransform;

        // Convert both corners to local space
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRect, screenStart, _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera, out Vector2 localStart);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRect, screenEnd, _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera, out Vector2 localEnd);

        // Calculate bottom-left and size
        Vector2 min = Vector2.Min(localStart, localEnd);
        Vector2 max = Vector2.Max(localStart, localEnd);
        Vector2 size = max - min;

        // Adjust for pivot
        Vector2 pivotOffset = new Vector2(_rect.pivot.x * size.x, _rect.pivot.y * size.y);
        _rect.anchoredPosition = min + pivotOffset;
        _rect.sizeDelta = size;
    }
}