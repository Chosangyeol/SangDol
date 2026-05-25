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
            { C_Enums.SkillSlot.Q, null },
            { C_Enums.SkillSlot.W, null },
            { C_Enums.SkillSlot.E, null },
            { C_Enums.SkillSlot.R, null },
            { C_Enums.SkillSlot.A, null },
            { C_Enums.SkillSlot.S, null },
            { C_Enums.SkillSlot.D, null },
            { C_Enums.SkillSlot.F, null },
            { C_Enums.SkillSlot.V, null  }
        };

    public SkillBase IdentitySkill { get; private set; }
    public SkillBase DodgeSkill { get; private set; }

    public event Action OnSkillDataChanged;

    private int useSkillPoint = 0;

    public C_SkillSystem(CharacterModel model)
    {
        _model = model;

        IdentitySkill = _model.skill_ZSO.SkillInit(_model);
        DodgeSkill = _model.skill_SpaceSO.SkillInit(_model);

        useSkillPoint = 0;
        return;
    }

    public virtual bool UpdateSkills(float deltaTime)
    {
        bool result = false;

        IdentitySkill?.UpdateSkill(deltaTime);
        DodgeSkill?.UpdateSkill(deltaTime);

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
        if (slot == C_Enums.SkillSlot.Z) return IdentitySkill;
        if (slot == C_Enums.SkillSlot.Space) return DodgeSkill;

        if (activeSkills.ContainsKey(slot))
        {
            return activeSkills[slot];
        }
        return null;
    }

    public void UseSkill(C_Enums.SkillSlot slot, Vector3 targetPos)
    {
        if (!_model.canMove) return;

        SkillBase targetSkill = GetSkillToSlot(slot);

        if (targetSkill != null)
        {
            if (targetSkill.UseSkill(targetPos))
            {
                if (slot == C_Enums.SkillSlot.Space)
                    _model.UseDodge();
            }
        }
        else
        {
            Debug.Log("해당 슬롯에 스킬 없음");
        }
    }

    public void ReleaseSkill(C_Enums.SkillSlot slot, Vector3 targetPos)
    {
        SkillBase targetSkill = GetSkillToSlot(slot);

        if (targetSkill != null)
        {
            targetSkill.ReleaseSkill(targetPos);
        }
    }

    public void RegisterSkillToSlot(C_Enums.SkillSlot slot, SkillBase skill)
    {
        if (slot == C_Enums.SkillSlot.Z || slot == C_Enums.SkillSlot.Space)
        {
            Debug.LogWarning("아이덴티티와 이동기 슬롯에는 다른 스킬을 장착할 수 없습니다.");
            return;
        }

        activeSkills[slot] = skill;
        Debug.Log(slot + " / " + activeSkills[slot]);
        OnSkillDataChanged?.Invoke();
    }

    public void ClearSkillSlot(C_Enums.SkillSlot slot)
    {
        if (slot == C_Enums.SkillSlot.Z || slot == C_Enums.SkillSlot.Space) return;

        activeSkills[slot] = null;
        OnSkillDataChanged?.Invoke();
    }

    public void Swap(C_Enums.SkillSlot from, C_Enums.SkillSlot to)
    {
        if (from == to) return;

        if (from == C_Enums.SkillSlot.Z || from == C_Enums.SkillSlot.Space ||
            to == C_Enums.SkillSlot.Z || to == C_Enums.SkillSlot.Space)
        {
            Debug.LogWarning("기본 내장 스킬은 스왑할 수 없습니다.");
            return;
        }

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
        if (_model.Stat.Stat.skillPoint < 1) return;

        SkillBase targetSkill = hasSkillData.Find(x => x == skill);

        if (targetSkill == null)
        {
            RegisterSkill(skill);
            targetSkill = hasSkillData.Find(x => x == skill);
        }

        _model.Stat.Stat.RemoveSkillPoint();
        useSkillPoint++;
        targetSkill.LevelUpSkill();
        OnSkillDataChanged?.Invoke();
    }

    public void LevelDownSkill(SkillBase skill)
    {
        SkillBase targetSkill = hasSkillData.Find(x => x == skill);

        if (targetSkill == null) return;
        _model.Stat.Stat.AddSkillPoint();
        useSkillPoint--;

        targetSkill.LevelDownSkill();
        
        if (targetSkill.SkillLevel <= 0)
            UnregisterSkill(targetSkill);
        
        OnSkillDataChanged?.Invoke();
    }

    public void ResetSkillCooldown()
    {
        foreach (var skill in activeSkills.Values)
        {
            // 슬롯에 스킬이 장착되어 있는 경우(null이 아닌 경우)에만 실행
            if (skill != null)
            {
                skill.ResetSkillCool();
            }
        }

        IdentitySkill?.ResetSkillCool();
        DodgeSkill?.ResetSkillCool();
    }

    public void ResetSkillLevel()
    {

    }
}
