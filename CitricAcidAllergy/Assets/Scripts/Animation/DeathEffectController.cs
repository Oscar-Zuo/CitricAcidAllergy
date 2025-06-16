using UnityEngine;

public class DeathEffectController : MonoBehaviour, IAnimationCallBack
{
    [SerializeField]
    private Vector2 scale = Vector2.one;

    private void Awake()
    {
        transform.localScale = scale;
    }

    void IAnimationCallBack.OnAnimationStart(Animator animator)
    {
        throw new System.NotImplementedException();
    }

    void IAnimationCallBack.OnAnimationEnd(Animator animator)
    {
        Destroy(gameObject);
    }
}
