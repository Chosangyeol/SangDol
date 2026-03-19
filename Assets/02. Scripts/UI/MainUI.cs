using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class MainUI : MonoBehaviour
{
    [SerializeField] Transform slotParent;
    public List<SkillSlot> skillSlots;
    public List<UseItemSlot> useItemSlots;
    public ItemTooltip tooltip;

    private C_SkillSystem _skillSystem;
    private C_Inventory _inventory;
    private CharacterModel _model;

    public void Init(C_SkillSystem skillSystem,C_Inventory inventory, CharacterModel model)
    {
        _skillSystem = skillSystem;
        _inventory = inventory;
        _model = model;

        foreach (var slot in skillSlots)
        {
            slot.Init(skillSystem);
            slot.Refresh();
        }

        foreach (var slot in useItemSlots)
        {
            slot.Init(inventory, tooltip);
            slot.Refresh();
        }
        
        BindSkillEvent();
    }

    private void BindSkillEvent()
    {
        _skillSystem.OnSkillDataChanged += RefreshAll;
    }

    private void RefreshAll()
    {
        foreach (var slot in skillSlots)
            slot.Refresh();
    }




}
