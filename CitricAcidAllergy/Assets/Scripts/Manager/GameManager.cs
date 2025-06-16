using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    private float _contributionValue = 0;

    [SerializeField]
    private int _maxRows = 10;

    [SerializeField]
    private ShipController _ship;

    [SerializeField]
    private Vector2 _gridSize = new(0.8f, 0.8f);

    [SerializeField]
    private Vector2 _mapYLimiation;

    [SerializeField]
    private float _sellFromShipMultiple = 1.1f;

    [SerializeField]
    private EnemySpawnPointController[] _enemySpawns;

    [SerializeField]
    private AnimationCurve _difficultyLevelCurve;

    [SerializeField]
    private NotificationPanelController _notificationPanel;

    [SerializeField]
    private DetailsPanelController _detailsPanel;

    [SerializeField]
    private ReinforcementPanelController _reinforcementPanel;

    [SerializeField]
    private RewardPanelController _rewardPanel;

    [SerializeField]
    private float _generateRewardChange = 0.5f;

    [SerializeField]
    private TMP_Text _contributionText;

    [SerializeField]
    private GameObject _pausedIndicator;

    [SerializeField]
    private float _gameLength = 300.0f;

    [SerializeField]
    private float _spawnHordeInterval = 10.0f;

    [SerializeField]
    private float _spawnEnemyInRowInterval = 1.0f;

    [SerializeField]
    private int _contributionLevelInterval = 50;

    private int _pauser = 0;
    private DateTime _startTime;

    private Coroutine _countDownCoroutine;

    #region Presets Settings
    private Dictionary<string, TurretPreset> _turretPresets;
    private readonly string _turretPresetDir = "Presets/Turrets";

    private Dictionary<string, BaseTurretPluginPreset> _pluginPresets;
    private readonly string _pluginPresetDir = "Presets/Plugins";

    #endregion

    #region Default Prefabs
    [Header("Prefabs")]
    [SerializeField]
    private GameObject _defaultEnemyPrefab;
    [SerializeField]
    private GameObject _defaultPlatformPrefab;
    [SerializeField]
    private GameObject _defaultProjectilePrefab;
    [SerializeField]
    private GameObject _defaultDeathEffectPrefab;
    [SerializeField]
    private GameObject _defaultTurretPrefab;
    #endregion

    private ProjectilePool _projectilePool;

    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindFirstObjectByType<GameManager>();
            }

            if (_instance == null)
            {
                Debug.LogError("Can't find GameManager!");
            }

            return _instance;
        }
    }

    #region Setters and Getters
    public EnemySpawnPointController[] EnemySpawns { get => _enemySpawns; }
    public ProjectilePool ProjectilePool { get => _projectilePool; }
    public Vector2 GridSize { get => _gridSize; }
    public ShipController Ship { get => _ship; }
    public Vector2 ShipSize { get => _gridSize * Ship.Size; }
    public int MaxRows { get => _maxRows; }
    public GameObject DefaultEnemyPrefab { get => _defaultEnemyPrefab; }
    public GameObject DefaultPlatformPrefab { get => _defaultPlatformPrefab; }
    public GameObject DefaultProjectilePrefab { get => _defaultProjectilePrefab; }
    public GameObject DefaultDeathEffectPrefab { get => _defaultDeathEffectPrefab; }
    public GameObject DefaultTurretPrefab { get => _defaultTurretPrefab; }
    public Dictionary<string, TurretPreset> TurretPresets { get => _turretPresets; }
    public Dictionary<string, BaseTurretPluginPreset> PluginPresets { get => _pluginPresets; }
    public Vector2 MapYLimiation { get => _mapYLimiation; }
    public bool IsPaused { get => _pauser > 0; }
    public float ContributionValue
    {
        get => _contributionValue; 
        protected set
        {
            _contributionValue = value;
            _contributionText.text = Mathf.Floor(_contributionValue).ToString();
        }
    }
    public NotificationPanelController NotificationPanel { get => _notificationPanel; }
    public DetailsPanelController DetailsPanel { get => _detailsPanel; }
    public float GameLength { get => _gameLength; }
    public float SpawnHordeInterval { get => _spawnHordeInterval; }
    public float SpawnEnemyInRowInterval { get => _spawnEnemyInRowInterval; }
    public ReinforcementPanelController ReinforcementPanel { get => _reinforcementPanel; }
    public RewardPanelController RewardPanel { get => _rewardPanel; }
    public float GenerateRewardChange { get => _generateRewardChange; }
    #endregion

    private void Awake()
    {
        Intialize();
    }

    private void Intialize()
    {
        _projectilePool = new();

        ContributionValue = 0f;

        _startTime = DateTime.Now;

        if (_mapYLimiation.x > _mapYLimiation.y)
        {
            (_mapYLimiation.x, _mapYLimiation.y) = (_mapYLimiation.y, _mapYLimiation.x);
        }

        InitializePresets();

        GiveInitalizalSoldiers();

        _countDownCoroutine = StartCoroutine(CountDown());
    }

    private IEnumerator CountDown()
    {
        yield return new WaitForSeconds(GameLength);
        PlayerPrefs.SetInt("Won", 1);
        GameOver();
    }

    private void GiveInitalizalSoldiers()
    {
        // RUNNING OUT OF TIME
        Dictionary<Vector2Int, TurretPreset> turret = new()
        {
            { new(0,0), TurretPresets["Level1 Soldier"] },
        };

        var initialReinforcement = new ReinforcementPreset(turret, ReinforcementSourceType.Others);

        for (int i = 0; i < 3; ++i)
        {
            _reinforcementPanel.AddReinforcement(initialReinforcement);
        }
    }

    private void InitializePresets()
    {
        _turretPresets = new();
        var loadedTurretPresets = Resources.LoadAll<TurretPresetScriptableObject>(_turretPresetDir);
        foreach (var preset in loadedTurretPresets)
        {
            _turretPresets.Add(preset.TurretPreset.presetName, preset.TurretPreset);
        }

        _pluginPresets = new();
        var loadedPluginPresets = Resources.LoadAll<PluginPresetScriptableObject>(_pluginPresetDir);
        foreach (var preset in loadedPluginPresets)
        {
            _pluginPresets.Add(preset.turretPluginPreset.presetName, preset.turretPluginPreset);
        }
    }

    public void PauseGame()
    {
        _pauser++;
        Time.timeScale = 0;
    }

    public void UnpauseGame()
    {
        _pauser--;
        if (_pauser <= 0)
        {
            Time.timeScale = 1;
            _pauser = 0;
        }
    }

    public void OnEditMode(InputAction.CallbackContext context)
    {
        if (IsPaused)
        {
            UnpauseGame();
            _pausedIndicator.SetActive(false);
        }
        else
        {
            PauseGame();
            _pausedIndicator.SetActive(true);
        }
    }

    public void SellReinforcement(ReinforcementPreset reinforcement)
    {
        if (reinforcement == null)
            return;

        if (reinforcement.sourceType == ReinforcementSourceType.Ship)
        {
            ContributionValue += reinforcement.TotalValue * _sellFromShipMultiple;
        }
        else
        {
            ContributionValue += reinforcement.TotalValue;
        }

        if (reinforcement.sourceType == ReinforcementSourceType.Ship)
        {
            var json = reinforcement.GetJson();
            LambdaManager.SubmitJson(json);
        }
    }

    public void SellPlugin(BaseTurretPluginPreset pluginPreset)
    {
        if (pluginPreset == null)
            return;

        ContributionValue += pluginPreset.value;
    }

    public float GetDifficultyLevelFromTime()
    {
        TimeSpan timeSpan = DateTime.Now - _startTime;

        return _difficultyLevelCurve.Evaluate((float)timeSpan.TotalSeconds);
    }

    public int GetRewardLevelBasedOnContribution()
    {
        return Mathf.FloorToInt(ContributionValue) / _contributionLevelInterval + 1;
    }

    public void GiveReward()
    {
        RewardPanel.Show();
        RewardPanel.gameObject.SetActive(true);
    }

    public void GameOver()
    {
        PlayerPrefs.SetFloat("ContributionValue", ContributionValue);
        SceneManager.LoadScene("GameOver");
    }
}
