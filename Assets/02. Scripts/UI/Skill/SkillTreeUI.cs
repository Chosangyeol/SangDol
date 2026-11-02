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
        UnbindSkillEvent();

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
        GameEvent.OnStatChange += HandleStatChanged;
    }

    private void UnbindSkillEvent()
    {
        if (_skillSystem != null)
            _skillSystem.OnSkillDataChanged -= RefreshAll;
        GameEvent.OnStatChange -= HandleStatChanged;
    }

    private void HandleStatChanged(CharacterStat stat)
    {
        if (_model != null && ReferenceEquals(stat, _model.Stat.Stat)) RefreshAll();
    }

    private void OnDestroy()
    {
        UnbindSkillEvent();
    }

    private void OnDisable()
    {
        HideSkillTooltip();
    }

    private void HideSkillTooltip()
    {
        if (_skillToolTip != null)
            _skillToolTip.ToggleSkillTooltip(false);
    }

    public void RefreshAll()
    {
        foreach (var slot in slots)
            slot.Refresh();

        remainSkillPoint.text = $"남은 포인트 : {_model.Stat.Stat.remainSkillPoint}";
        totalSkillPoint.text = $"전체 포인트 : {_model.Stat.Stat.totalSkillPoint}";
    }

    internal bool ApplyPortalLoadout()
    {
        if (_skillSystem == null || slots.Count < 4) return false;

        C_Enums.SkillSlot[] targetSlots =
        {
            C_Enums.SkillSlot.Q,
            C_Enums.SkillSlot.W,
            C_Enums.SkillSlot.E,
            C_Enums.SkillSlot.R
        };

        for (int i = 0; i < targetSlots.Length; i++)
        {
            SkillBase skill = slots[i].CurrentSkill;
            if (skill == null || !_skillSystem.MeetsRequiredLevel(skill)) return false;
            if (skill.SkillLevel == 0)
            {
                if (!_skillSystem.CanLevelUpSkill(skill)) return false;
                _skillSystem.LevelUpSkill(skill);
            }

            if (skill.SkillLevel > 0)
                _skillSystem.RegisterSkillToSlot(targetSlots[i], skill);
        }

        RefreshAll();
        return true;
    }

    public void Toggle(bool onlyFalse = false)
    {
        HideSkillTooltip();

        if (onlyFalse)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(!gameObject.activeSelf);
    }
}
