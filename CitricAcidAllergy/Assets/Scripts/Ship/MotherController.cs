using UnityEngine;
using UnityEngine.SceneManagement;

public class MotherController : MonoBehaviour
{
    [SerializeField]
    private int _maxHealth = 10;

    private int _currentHealth;

    public int MaxHealth { get => _maxHealth; }
    public int CurrentHealth { get => _currentHealth; }

    public void Initialize()
    {
        _currentHealth = MaxHealth;
    }

    public void OnHitByEnemy()
    {
        _currentHealth--;
        if (CurrentHealth < 0)
        {
            GameManager.Instance.GameOver();
        }
    }
}
