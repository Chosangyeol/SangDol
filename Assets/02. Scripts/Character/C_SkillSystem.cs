using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class C_SkillSystem
{
    private readonly CharacterModel _model;

    protected List<SkillBase> hasSkillData = new();

    protected Dictionary<C_Enums.SkillSlot, SkillBase> activeSkills = 
        new Dictionary<C_Enums.SkillSlot, SkillBase>()
        {
            { C_Enums.SkillSlot.Z, null },
            { C_Enums.SkillSlot.Q, null },
            { C_Enums.SkillSlot.W, null },
            { C_Enums.SkillSlot.E, null },
            { C_Enums.SkillSlot.R, null },
            { C_Enums.SkillSlot.Space, null},
            { C_Enums.SkillSlot.V, null  }
        };

    public event Action OnSkillDataChanged;

    public C_SkillSystem(CharacterModel model)
    {
        _model = model;

        SkillBase iden = _model.skill_ZSO.SkillInit(_model);
        RegisterSkill(iden);
        RegisterSkillToSlot(C_Enums.SkillSlot.Z, iden);

        SkillBase space = _model.skill_SpaceSO.SkillInit(_model);
        RegisterSkill(space);
        RegisterSkillToSlot(C_Enums.SkillSlot.Space, space);

        Debug.Log(activeSkills[C_Enums.SkillSlot.Z]);
        Debug.Log(activeSkills[C_Enums.SkillSlot.Space]);

        return;
    }

    public virtual bool UpdateSkills(float deltaTime)
    {
        bool result = false;

        foreach (var skillPair in activeSkills)
        {
            if (skillPair.Value != null)
            {
                skillPair.Value.UpdateSkill(deltaTime);
            }
        }
        return result;
    }

    public SkillBase GetSkillToSlot(C_Enums.SkillSlot slot)
    {
        if (activeSkills.ContainsKey(slot))
        {
            Debug.Log(activeSkills[slot]);
            return activeSkills[slot];
        }
        return null;
    }

    public void UseSkill(C_Enums.SkillSlot slot, Vector3 targetPos)
    {
        if (activeSkills[slot] != null)
            activeSkills[slot].UseSkill(targetPos);
        else
            Debug.Log("해당 슬롯에 스킬 없음");

    }

    public void RegisterSkillToSlot(C_Enums.SkillSlot slot, SkillBase skill)
    {
        activeSkills[slot] = skill;
        Debug.Log(slot + " / " + activeSkills[slot]);
        OnSkillDataChanged?.Invoke();
    }

    public void ClearSkillSlot(C_Enums.SkillSlot slot)
    {
        activeSkills[slot] = null;
        OnSkillDataChanged?.Invoke();
    }

    public void Swap(C_Enums.SkillSlot from, C_Enums.SkillSlot to)
    {
        if (from == to) return;

        SkillBase fromSkill = GetSkillToSlot(from);
        SkillBase toSkill = GetSkillToSlot(to);
        activeSkills[from] = toSkill;
        activeSkills[to] = fromSkill;
    }

    public void RegisterSkill(SkillBase skill)
    {
        if (hasSkillData.Contains(skill)) return;

        hasSkillData.Add(skill);
        OnSkillDataChanged?.Invoke();
    }

    public void UnregisterSkill(SkillBase skill)
    {
        if (!hasSkillData.Contains(skill)) return;

        hasSkillData.Remove(skill);
        OnSkillDataChanged?.Invoke();
    }

    public void LevelUpSkill(SkillBase skill)
    {
        SkillBase targetSkill = hasSkillData.Find(x => x == skill);

        if (targetSkill == null)
        {
            RegisterSkill(skill);
            targetSkill = hasSkillData.Find(x => x == skill);
        }

        targetSkill.LevelUpSkill();
        OnSkillDataChanged?.Invoke();
    }

    public void LevelDownSkill(SkillBase skill)
    {
        SkillBase targetSkill = hasSkillData.Find(x => x == skill);

        if (targetSkill == null) return;
        
        targetSkill.LevelDownSkill();
        
        if (targetSkill.SkillLevel <= 0)
            UnregisterSkill(targetSkill);
        
        OnSkillDataChanged?.Invoke();
    }
}
