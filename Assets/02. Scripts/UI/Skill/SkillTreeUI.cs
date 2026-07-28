using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SkillTreeUI : MonoBehaviour
{
    public SkillDataListSO skillList;

    [SerializeField] SkillTreeSlot slotPrefab;
    [SerializeField] Transform slotParent;

    [Header("텍스트")]
    public TMP_Text remainSkillPoint;
    public TMP_Text totalSkillPoint;

    private CharacterModel _model;
    private C_SkillSystem _skillSystem;
    private List<SkillTreeSlot> slots = new();

    private SkillToolTip _skillToolTip;

    public void Init(C_SkillSystem skillSystem, CharacterModel model, SkillToolTip skillToolTip)
    {
        _model = model;
        _skillSystem = skillSystem;
        _skillToolTip = skillToolTip;

        for (int i = 0; i < skillList.skillList.Count; i++)
        {
            SkillTreeSlot slot = Instantiate(slotPrefab, slotParent);
            slot.Init(skillSystem, skillList.skillList[i].SkillInit(_model), _skillToolTip);
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

        remainSkillPoint.text = $"남은 포인트 : {_model.Stat.Stat.remainSkillPoint}";
        totalSkillPoint.text = $"전체 포인트 : {_model.Stat.Stat.totalSkillPoint}";
    }

    public void Toggle(bool onlyFalse = false)
    {
        if (onlyFalse)
        {
            gameObject.SetActive(false);
            return;
        }

        _skillToolTip.ToggleSkillTooltip(false);
        gameObject.SetActive(!gameObject.activeSelf);
    }
}
