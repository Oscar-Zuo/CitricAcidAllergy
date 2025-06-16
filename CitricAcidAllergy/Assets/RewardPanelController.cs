using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class RewardPanelController : MonoBehaviour
{
    public TMP_Text option1Description;

    public RawImage option2Preview;
    public TMP_Text option2Description;

    private ReinforcementPreset option2Preset;
    private Dictionary<string, int> option1Turrets;
    private Dictionary<string, int> option1Plugins;

    public void Show()
    {
        LambdaManager.FetchJson(GameManager.Instance.GetRewardLevelBasedOnContribution(), Initialize);
    }

    public void Initialize(ReinfocementSerializeStruct? reinfocementSerializeStruct)
    {

        var rewardLevel = GameManager.Instance.GetRewardLevelBasedOnContribution();
        InitialzeOption1(rewardLevel);
        if (reinfocementSerializeStruct == null)
        {
            GameManager.Instance.NotificationPanel.ShowMessage("Something went wrong", "Something went wrong, if the issue persists, contact oscar_zuo@outlook.com", null);
            return;
        }
        else
        {
            InitialzeOption2(reinfocementSerializeStruct.Value);
        }
        gameObject.SetActive(true);
    }

    private void OnEnable()
    {
        GameManager.Instance.PauseGame();
    }

    private void OnDisable()
    {
        GameManager.Instance.UnpauseGame();
    }

    private void InitialzeOption1(int rewardLevel)
    {
        int option1TurretCount = Random.Range(0, rewardLevel + 1);
        int option1PluginsCount = rewardLevel - option1TurretCount;

        option1Turrets = new();
        List<string> turretNames = new(GameManager.Instance.TurretPresets.Keys);

        for (int i = 0; i < option1TurretCount; ++i)
        {
            var randomName = turretNames[Random.Range(0, turretNames.Count)];
            if (option1Turrets.ContainsKey(randomName))
            {
                option1Turrets[randomName] += 1;
            }
            else
            {
                option1Turrets[randomName] = 1;
            }
        }

        option1Plugins = new();
        List<string> pluginNames = new(GameManager.Instance.PluginPresets.Keys);

        for (int i = 0; i < option1PluginsCount; ++i)
        {
            var randomName = pluginNames[Random.Range(0, pluginNames.Count)];
            if (option1Plugins.ContainsKey(randomName))
            {
                option1Plugins[randomName] += 1;
            }
            else
            {
                option1Plugins[randomName] = 1;
            }
        }

        var description = "";
        if (option1TurretCount>0)
        {
            foreach (var turret in option1Turrets)
            {
                description += $"{turret.Key} * {turret.Value}\n";
            }
        }

        if (option1PluginsCount > 0)
        {
            foreach (var plugin in option1Plugins)
            {
                description += $"{plugin.Key} * {plugin.Value}\n";
            }
        }

        option1Description.text = description;
    }

    private void InitialzeOption2(ReinfocementSerializeStruct reinfocementSerializeStruct)
    {
        var preset = ReinforcementPreset.GenerateReinforcementPresetFromStuct(reinfocementSerializeStruct);
        option2Preset = preset;
        option2Description.text = option2Preset.GetDescription();

        (Vector2Int, Sprite)[] input = new (Vector2Int, Sprite)[preset.Turrets.Count];
        int cnt = 0;
        foreach (var turretPresetPair in preset.Turrets)
        {
            input[cnt++] = new(turretPresetPair.Key, turretPresetPair.Value.previewSprite);
        }

        option2Preview.texture = ThumbnailGenerater.GenerateThumbnail(input, 256);
    }

    public void OnClickedOption1()
    {
        foreach (var turretPair in option1Turrets)
        {
            TurretPreset preset = GameManager.Instance.TurretPresets[turretPair.Key];
            Dictionary<Vector2Int, TurretPreset> turret = new()
            {
                { new(0,0), preset },
            };

            GameManager.Instance.ReinforcementPanel.AddReinforcement(new ReinforcementPreset(turret, ReinforcementSourceType.Others));
        }

        foreach (var pluginPair in option1Plugins)
        {
            BaseTurretPluginPreset preset = GameManager.Instance.PluginPresets[pluginPair.Key];

            GameManager.Instance.ReinforcementPanel.AddPlugin(preset);
        }

        gameObject.SetActive(false);
    }

    public void OnClickedOption2()
    {
        if (option2Preset != null)
        {
            GameManager.Instance.ReinforcementPanel.AddReinforcement(option2Preset);
        }
        option2Preset = null;
        gameObject.SetActive(false);
    }
}
