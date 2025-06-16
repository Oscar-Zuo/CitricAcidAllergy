using System;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class BaseTurretPluginPreset
{
    public string presetName;
    public Sprite previewSprite;

    [Range(0, 100)]
    public float turretShootingPlusModifier = 0;
    [Range(0, 10)]
    public float turretShootingMultipleModifier = 0;

    [Range(0, 100)]
    public float damagePlusModifier = 0;
    [Range(0, 10)]
    public float damageMultipleModifier = 0;

    [Range(0, 100)]
    public float speedPlusModifier = 0;
    [Range(0, 10)]
    public float speedMultipleModifier = 0;

    [Range(-1, 10)]
    public int penetrationPlusModifier = 0;

    [Range(0, 10)]
    public ushort level = 1;

    public int size = 1;

    public float value;

    [TextArea]
    public string description;

    public virtual BaseTurretPluginPreset CopyFromThis()
    {
        BaseTurretPluginPreset newPreset = new()
        {
            presetName = this.presetName,
            previewSprite = this.previewSprite,
            turretShootingPlusModifier = this.turretShootingPlusModifier,
            turretShootingMultipleModifier = this.turretShootingMultipleModifier,
            damagePlusModifier = this.damagePlusModifier,
            damageMultipleModifier = this.damageMultipleModifier,
            penetrationPlusModifier = this.penetrationPlusModifier,
            speedMultipleModifier = this.speedMultipleModifier,
            speedPlusModifier = this.speedPlusModifier,
            level = this.level,
            value = this.value,
            description = this.description
        };

        return newPreset;
    }
}
