using UnityEngine;

public class AnimationBridge : MonoBehaviour
{
    [SerializeField]
    private MonoBehaviour _targetComponent;
    private IAnimationCallBack _controllerComponent;

    public IAnimationCallBack ControllerComponent { get => _controllerComponent;}

    private void Awake()
    {
        _controllerComponent = (IAnimationCallBack)_targetComponent;

        if (_controllerComponent == null )
        {
            Debug.Log("Component doesn't implenment ILiving!");
        }
    }
}
