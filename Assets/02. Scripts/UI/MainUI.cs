using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class MainUI : MonoBehaviour
{
    [SerializeField] Transform slotParent;
    public List<SkillSlot> skillSlots;

    private C_SkillSystem _skillSystem;
    private CharacterModel _model;

    public void Init(C_SkillSystem skillSystem, CharacterModel model)
    {
        _skillSystem = skillSystem;
        _model = model;

        foreach (var slot in skillSlots)
        {
            slot.Init(skillSystem);
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
