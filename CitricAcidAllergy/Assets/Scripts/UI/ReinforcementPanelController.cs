using UnityEngine;

public class ReinforcementPanelController : MonoBehaviour
{
    public Transform contentTransform;

    public GameObject reinforcementGridPrefab;
    public GameObject pluginGridPrefab;


    public void AddReinforcement(ReinforcementPreset preset)
    {
        if (preset == null)
            return;
        var newGrid = Instantiate(reinforcementGridPrefab, contentTransform);
        var controller = newGrid.GetComponent<ReinforcementGridController>();
        controller.Initialize(preset);
    }


    public void AddPlugin(BaseTurretPluginPreset preset)
    {
        if (preset == null)
            return;
        var newGrid = Instantiate(pluginGridPrefab, contentTransform);
        var controller = newGrid.GetComponent<PluginGridController>();
        controller.Initialize(preset);
    }
}
