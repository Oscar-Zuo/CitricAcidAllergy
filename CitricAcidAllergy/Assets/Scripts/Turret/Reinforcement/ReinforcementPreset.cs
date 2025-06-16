using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public enum ReinforcementSourceType
{
    None,
    Ship,
    Others,
}

[Serializable]
public struct ReinfocementSerializeStruct
{
    [Serializable]
    public struct TurretSerializeStruct
    {
        [SerializeField]
        public List<string> plugins;
        public string name;
        public Vector2Int grid;
    }
    public string senderName;
    public float value;
    [SerializeField]
    public List<TurretSerializeStruct> turrets;

    public ReinfocementSerializeStruct(string senderName, float value, Tuple<Vector2Int, TurretPreset>[] turrets)
    {
        this.senderName = senderName;
        this.value = value;

        this.turrets = new List<TurretSerializeStruct>();
        foreach (var turretTuple in turrets)
        {
            this.turrets.Add(new TurretSerializeStruct()
            {
                plugins = turretTuple.Item2.pluginPresetNames.ToList(),
                name = turretTuple.Item2.presetName,
                grid = turretTuple.Item1
            });
        }
    }
}

[Serializable]
public class ReinforcementPreset
{
    // Key:Grid     Value: TurretPrest
    [SerializeField]
    private Dictionary<Vector2Int, TurretPreset> _turrets;

    private float _totalValue = 0f;
    private string _sender = "Mothership";

    public float TotalValue => _totalValue;

    public ReinforcementSourceType sourceType;

    public Dictionary<Vector2Int, TurretPreset> Turrets { get => _turrets; }
    public string Sender { get => _sender; }

    public ReinforcementPreset(Dictionary<Vector2Int, TurretPreset> turrets, ReinforcementSourceType source, string sender = "Mothership")
    {
        _turrets = new();

        foreach (var turret in turrets)
        {
            _turrets[turret.Key] = turret.Value.CopyFromThis();
        }

        sourceType = source;
        _totalValue = CalculateValue();
        _sender = sender;

        //Debug.Log(GetJson());
    }

    private float CalculateValue()
    {
        float totalValue = 0f;
        foreach (TurretPreset turretPreset in _turrets.Values)
        {
            totalValue += turretPreset.Value;
        }
        return totalValue;
    }

    public string GetDescription()
    {
        string description = string.Empty;
        description += $"Sent from:{Sender}\n";

        foreach (var turret in Turrets.Values)
        {
            description += turret.presetName + ":\n" + turret.GetDetails();
        }
        return description;
    }

    public string GetJson()
    {
        string sender = PlayerPrefs.GetString("SenderName");
        if (string.IsNullOrEmpty(sender))
            sender = "Oscar";

        Tuple<Vector2Int, TurretPreset>[] turretArray = _turrets
            .Select(kvp => Tuple.Create(kvp.Key, kvp.Value))
            .ToArray();

        var serializeStruct = new ReinfocementSerializeStruct(sender, _totalValue, turretArray);
        return JsonUtility.ToJson(serializeStruct);
    }

    public static ReinforcementPreset GenerateReinforcementPresetFromStuct(ReinfocementSerializeStruct structure)
    {
        Dictionary<Vector2Int, TurretPreset> turretDictionary = new();

        foreach (var turretStruct in structure.turrets) 
        {
            turretDictionary.Add(turretStruct.grid, GameManager.Instance.TurretPresets[turretStruct.name]);
        }

        var result = new ReinforcementPreset(turretDictionary, ReinforcementSourceType.Others, structure.senderName);
        return result;
    }
}
