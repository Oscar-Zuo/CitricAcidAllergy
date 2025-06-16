using UnityEngine;

[CreateAssetMenu(fileName = "TurretPresetScriptableObject", menuName = "Scriptable Objects/TurretPresetScriptableObject")]
public class TurretPresetScriptableObject : ScriptableObject
{
    [SerializeField]
    private TurretPreset _turretPreset;

    public TurretPreset TurretPreset => _turretPreset;
}
