using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StigmaUI : MonoBehaviour
{
    [Header("스티그마 노드들")]
    public List<StigmaSlotUI> allNodes;

    private CharacterModel _model;

    public void Init(CharacterModel model)
    {
        _model = model;

        foreach (var node in allNodes)
        {
            node.Init(this);
        }
        RefreshAllNodes();
    }

    public void OnNodeClicked(StigmaSlotUI clickedNode)
    {
        int level = clickedNode.stigmaLevel;
        EStigmaType type = clickedNode.stigamType;

        if (_model.Stat.Stat.currentLevel < level)
        {
            return;
        }

        C_Stigma stigma = _model.Stigma;

        if (stigma.selectedStigmas.TryGetValue(level, out EStigmaType currentType) && currentType == type)
        {
            stigma.UnEquipStigma(level);
        }
        else
        {
            stigma.EquipStigma(level, type);
        }

        RefreshAllNodes();
    }

    public void RefreshAllNodes()
    {
        if (_model == null || _model.Stigma == null) return;

        C_Stigma stigma = _model.Stigma;

        foreach(var node in allNodes)
        {
            bool isEquiped = stigma.selectedStigmas.ContainsValue(node.stigamType);

            bool isLocked = _model.Stat.Stat.currentLevel < node.stigmaLevel;

            node.UpdateVisual(isEquiped, isLocked);
        }
    }

    
}
