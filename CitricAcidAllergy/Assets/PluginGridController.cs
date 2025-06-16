using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PluginGridController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField]
    private Image thumbnailImage;

    private BaseTurretPluginPreset _pluginPreset;

    public BaseTurretPluginPreset PluginPreset { get => _pluginPreset; }

    public virtual void Initialize(BaseTurretPluginPreset newPluginPreset)
    {
        _pluginPreset = newPluginPreset;

        thumbnailImage.sprite = _pluginPreset.previewSprite;
    }

    public void OnButtonClicked()
    {

    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        DraggingManager.Instance.DragPluginPreset(PluginPreset, eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        DraggingManager.Instance.OnDraggingPluginPreset(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (DraggingManager.Instance.DropPluginPreset(eventData))
        {
            Destroy(gameObject);
        }
    }

    public void SellPlugin()
    {
        GameManager.Instance.SellPlugin(_pluginPreset);
        Destroy(gameObject);
    }

    public void MouseStartHover()
    {
        if (DraggingManager.Instance.IsPreviewShowing)
        {
            return;
        }

        string description = _pluginPreset.description;

        GameManager.Instance.DetailsPanel.Initialize(_pluginPreset.presetName, description, _pluginPreset.value.ToString());
        GameManager.Instance.DetailsPanel.gameObject.SetActive(true);
    }

    public void MouseEndHover()
    {
        GameManager.Instance.DetailsPanel.gameObject.SetActive(false);
    }
}
