using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider2D))]
public class BaseTurretController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private GameObject _projectileGameObject;
    [SerializeField]
    private Transform _projectilePivot;
    [SerializeField]
    private Animator _turretAnimator;

    private AnimatorOverrideController _animatorOverrideController;

    private Collider2D _collider;

    private TurretPreset _preset;
    private List<BaseTurretPluginPreset> _plugins = new();

    private float _currentHeath;
    private int _currentPluginSizeRemains;

    protected Coroutine _shootingCoroutine;

    public TurretPreset DefaultPreset 
    { 
        get => _preset; 
        protected set
        {
            _preset = value;
        }
    }

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();

        _animatorOverrideController = new(_turretAnimator.runtimeAnimatorController);
    }

    private void Start()
    {
        Activate();
    }

    public virtual void Initialize(TurretPreset preset)
    {
        // New one, don't use the original one
        DefaultPreset = preset.CopyFromThis();
        _currentHeath = DefaultPreset.maxniumHealth;
        _currentPluginSizeRemains = DefaultPreset.maxPluginSize;
        _animatorOverrideController["Idle"] = DefaultPreset.idleClip;

        InitializePlugins(DefaultPreset.pluginPresetNames);
    }

    protected void InitializePlugins(string[] pluginPresetsNames)
    {
        _plugins.Clear();

        if (pluginPresetsNames == null)
        {
            return;
        }

        foreach (var pluginName in pluginPresetsNames)
        {
            AddPlugin(pluginName);
        }
    }

    public TurretPreset GenerateCurrentPreset()
    {
        TurretPreset currentPreset = DefaultPreset.CopyFromThis();
        currentPreset.pluginPresetNames = new string[_plugins.Count];


        for (int i = 0; i < _plugins.Count;++i)
        {
            currentPreset.pluginPresetNames[i] = _plugins[i].presetName;
        }

        return currentPreset;
    }

    public bool AddPlugin(string pluginName)
    {
        if (GameManager.Instance.PluginPresets.TryGetValue(pluginName, out var pluginPreset))
        {
            return AddPlugin(pluginPreset);
        }
        else
        {
            Debug.LogWarning($"Plugin {pluginName} not found!");
            return false;
        }
    }

    public bool AddPlugin(BaseTurretPluginPreset pluginPreset)
    {
        if (!CanPlacePlugin(pluginPreset))
        {
            return false;
        }

        _plugins.Add(pluginPreset.CopyFromThis());
        _currentPluginSizeRemains -= pluginPreset.size;
        return true;
    }

    public bool CanPlacePlugin(BaseTurretPluginPreset pluginPreset)
    {
        return _currentPluginSizeRemains >= pluginPreset.size;
    }

    public void RemovePlugin(int pluginIndex)
    {
        if (pluginIndex < 0 || pluginIndex >= _plugins.Count || _plugins[pluginIndex] == null)
        {
            return;
        }

        _currentPluginSizeRemains += _plugins[pluginIndex].size;
        _plugins.RemoveAt(pluginIndex);
    }

    public void Activate()
    {
        if (_shootingCoroutine != null)
        {
            return;
        }

        _shootingCoroutine = StartCoroutine(ShootCoroutine());
    }

    public void Deactivate()
    {
        if (_shootingCoroutine == null) 
        {
            return;
        }

        StopCoroutine(_shootingCoroutine);
    }

    protected virtual IEnumerator ShootCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(GetShootingInterval());
            Shoot();
        }
    }

    protected virtual GameObject GetProjectileGameObject()
    {
        return _projectileGameObject;
    }

    protected virtual ProjectilePreset GenerateProjectilePreset()
    {
        var newPreset = new ProjectilePreset();
        newPreset.Intialize(GetProjectilePentration(newPreset), GetProjectileSpeed(newPreset), GetProjectileDamage(newPreset));
        return newPreset;
    }

    protected void Shoot()
    {
        var projectileController = GameManager.Instance.ProjectilePool.Get(GetProjectileGameObject());
        InitializeProjectile(projectileController);
    }

    protected void InitializeProjectile(BaseProjectileController projectileController)
    {
        projectileController.Initialize(_projectilePivot, this, GenerateProjectilePreset());
    }

    protected virtual float GetShootingInterval()
    {
        float plusModifier = 0;
        float multipleModifier = 1;

        foreach (var plugin in _plugins)
        {
            plusModifier += plugin.turretShootingPlusModifier;
            multipleModifier += plugin.turretShootingMultipleModifier;
        }

        float resultShooting = (DefaultPreset.baseShooting + plusModifier) * multipleModifier;

        return 1.0f / resultShooting;
    }

    protected virtual int GetProjectilePentration(ProjectilePreset currentPreset)
    {
        int plusModifier = 0;

        foreach (var plugin in _plugins)
        {
            if (plugin.penetrationPlusModifier < 0)
            {
                return -1;
            }

            plusModifier += plugin.penetrationPlusModifier;
        }

        return currentPreset.defaultPenetration + plusModifier;
    }

    protected virtual float GetProjectileDamage(ProjectilePreset currentPreset)
    {
        float plusModifier = 0;
        float multipleModifier = 1;

        foreach (var plugin in _plugins)
        {
            multipleModifier += plugin.damageMultipleModifier;
            plusModifier += plugin.damagePlusModifier;
        }

        return (currentPreset.defaultDamage + plusModifier) * multipleModifier;
    }

    protected virtual float GetProjectileSpeed(ProjectilePreset currentPreset)
    {
        float plusModifier = 0;
        float multipleModifier = 1;

        foreach (var plugin in _plugins)
        {
            multipleModifier += plugin.speedPlusModifier;
            plusModifier += plugin.speedMultipleModifier;
        }

        return (currentPreset.defaultSpeed + plusModifier) * multipleModifier;
    }

    public void OnHitByEnemy()
    {
        _currentHeath--;
        if (_currentHeath <= 0)
        {
            SelfDestroy();
        }
    }

    public virtual void SelfDestroy()
    {
        Deactivate();
        Destroy(gameObject);
    }

    public virtual string GetDetails()
    {
        var details = string.Empty;
        foreach (var plugin in _plugins)
        {
            details += $" -{plugin.presetName}\n";
        }
        return details;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (DraggingManager.Instance.IsPreviewShowing)
        {
            return;
        }

        GameManager.Instance.DetailsPanel.Initialize(_preset.presetName, GetDetails(), _preset.Value.ToString());
        GameManager.Instance.DetailsPanel.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GameManager.Instance.DetailsPanel.gameObject.SetActive(false);
    }
}
