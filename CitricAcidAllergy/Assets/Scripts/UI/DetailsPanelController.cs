using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DetailsPanelController : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _nameText;

    [SerializeField]
    private TMP_Text _descriptionText;

    [SerializeField]
    private TMP_Text _valueText;

    public void Initialize(string name, string description, string value)
    {
        _nameText.text = name;
        _descriptionText.text = description;
        _valueText.text = value;
    }

    private void Update()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent as RectTransform, mousePos, null, out Vector2 localPos);

        transform.localPosition = localPos + new Vector2(10,10);
    }
}
