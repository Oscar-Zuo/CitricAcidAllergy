using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class ReinforcementGridController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField]
    private RawImage thumbnailImage;

    private ReinforcementPreset _reinforcementPreset;

    public ReinforcementPreset ReinforcementPreset { get => _reinforcementPreset; }

    public virtual void Initialize(ReinforcementPreset reinforcementPreset)
    {
        _reinforcementPreset = reinforcementPreset;

        var thumbnail = GenerateThumbnail(_reinforcementPreset);
        thumbnailImage.texture = thumbnail;
    }

    protected virtual Texture2D GenerateThumbnail(ReinforcementPreset reinforcementPreset)
    {
        (Vector2Int, Sprite)[] input = new (Vector2Int, Sprite)[reinforcementPreset.Turrets.Count];
        int cnt = 0;
        foreach (var turretPresetPair in reinforcementPreset.Turrets)
        {
            input[cnt++] = new(turretPresetPair.Key, turretPresetPair.Value.previewSprite);
        }

        return ThumbnailGenerater.GenerateThumbnail(input, 128);
    }

    public void OnButtonClicked()
    {

    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        DraggingManager.Instance.DragReinforcementPreset(_reinforcementPreset, eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        DraggingManager.Instance.OnDraggingReinforcementPreset(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (DraggingManager.Instance.DropReinforcementPreset(eventData))
        {
            Destroy(gameObject);
        }
    }

    public void SellReinforcement()
    {
        GameManager.Instance.SellReinforcement(_reinforcementPreset);
        Destroy(gameObject);
    }

    public void MouseStartHover()
    {
        if (DraggingManager.Instance.IsPreviewShowing)
        {
            return;
        }

        string description = $"Given by {_reinforcementPreset.Sender}, contains:\n{_reinforcementPreset.GetDescription()}";

        GameManager.Instance.DetailsPanel.Initialize("Reinforcement", description, _reinforcementPreset.TotalValue.ToString());
        GameManager.Instance.DetailsPanel.gameObject.SetActive(true);
    }

    public void MouseEndHover()
    {
        GameManager.Instance.DetailsPanel.gameObject.SetActive(false);
    }
}
