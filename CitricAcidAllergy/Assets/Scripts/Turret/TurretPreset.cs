using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class TurretPreset
{
    public string presetName;
    public Sprite previewSprite;
    public AnimationClip idleClip;

    public float baseShooting = 1.5f;
    public float maxniumHealth = 3.0f;
    public float selfValue;
    public float Value => CalculateValue();

    [TextArea]
    public string description;

    public int maxPluginSize = 3;
    public string[] pluginPresetNames;

    public virtual TurretPreset CopyFromThis()
    {
        TurretPreset preset = new()
        {
            presetName = this.presetName,
            previewSprite = this.previewSprite,
            idleClip = this.idleClip,
            baseShooting = this.baseShooting,
            maxniumHealth = this.maxniumHealth,
            description = this.description,
            maxPluginSize = this.maxPluginSize,
            pluginPresetNames = new string[this.pluginPresetNames.Length],
            selfValue = this.selfValue
        };

        Array.Copy(this.pluginPresetNames, preset.pluginPresetNames, this.pluginPresetNames.Length); 
        return preset;
    }

    protected float CalculateValue()
    {
        var totalValue = selfValue;

        foreach (var pluginName in pluginPresetNames)
        {
            totalValue += GameManager.Instance.PluginPresets[pluginName].value;
        }

        return totalValue;
    }

    public virtual string GetDetails()
    {
        var details = string.Empty;
        foreach (var pluginName in pluginPresetNames)
        {
            details += $" -{pluginName}\n";
        }
        return details;
    }
}
