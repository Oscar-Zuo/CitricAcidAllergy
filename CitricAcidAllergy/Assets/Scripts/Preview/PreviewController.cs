using UnityEngine;

public class PreviewController : MonoBehaviour
{
    [SerializeField]
    private Color _defaultColor = Color.white;
    [SerializeField]
    private Color _validColor = Color.green;
    [SerializeField]
    private Color _invalidColor = Color.red;
    [SerializeField]
    [Range(0, 1)]
    private float _alpha = 0.5f;


    [SerializeField]
    private SpriteRenderer _renderer;

    public void Awake()
    {
        _defaultColor.a = _alpha;
        _validColor.a = _alpha;
        _invalidColor.a = _alpha;
    }

    public void SetSprite(Sprite sprite)
    {
        _renderer.sprite = sprite;
    }

    public void SwitchToDefault()
    {
        _renderer.color = _defaultColor;
    }
    public void SwitchToValid()
    {
        _renderer.color = _validColor;
    }

    public void SwitchToInvalid()
    {
        _renderer.color = _invalidColor;
    }
}
