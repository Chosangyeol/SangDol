using System.Collections.Generic;
using UnityEngine;

public class SkillTreeUI : MonoBehaviour
{
    public SkillDataListSO skillList;

    [SerializeField] SkillTreeSlot slotPrefab;
    [SerializeField] Transform slotParent;

    private CharacterModel _model;
    private C_SkillSystem _skillSystem;
    private List<SkillTreeSlot> slots = new();

    public void Init(C_SkillSystem skillSystem, CharacterModel model)
    {
        _model = model;
        _skillSystem = skillSystem;

        for (int i = 0; i < skillList.skillList.Count; i++)
        {
            SkillTreeSlot slot = Instantiate(slotPrefab, slotParent);
            slot.Init(skillSystem, skillList.skillList[i].SkillInit(_model));
            slots.Add(slot);
        }

        BindSkillEvent();
        RefreshAll();
    }

    private void BindSkillEvent()
    {
        _skillSystem.OnSkillDataChanged += RefreshAll;
    }

    public void RefreshAll()
    {
        foreach (var slot in slots)
            slot.Refresh();
    }

    public void Toggle()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
}
