using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MainCameraController : MonoBehaviour
{
    private Camera _camera;

    private Vector2 _cameraYLimits;

    public float smoothTime = 0.3f;
    private float _velocityY = 0f;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    private void Start()
    {
        _cameraYLimits = GetCameraYLimits();
    }

    private Vector2 GetCameraYLimits()
    {
        float halfHeight = _camera.orthographicSize;

        float minY = GameManager.Instance.MapYLimiation.x;
        float maxY = GameManager.Instance.MapYLimiation.y;

        var result = new Vector2(minY + halfHeight, maxY - halfHeight);
        if (result.x > result.y)
        {
            result = new Vector2((minY + maxY) / 2, (minY + maxY) / 2);
        }
        return result;
    }


    private void LateUpdate()
    {
        float shipY = GameManager.Instance.Ship.transform.position.y;

        float targetY = Mathf.Clamp(shipY, _cameraYLimits.x, _cameraYLimits.y);

        float smoothedY = Mathf.SmoothDamp(
            current: transform.position.y,
            target: targetY,
            currentVelocity: ref _velocityY,
            smoothTime: smoothTime
        );

        transform.position = new Vector3(
            transform.position.x,
            smoothedY,
            transform.position.z
        );
    }
}
