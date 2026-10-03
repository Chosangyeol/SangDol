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
    private SkillBase bufferedSkill;
    private C_Enums.SkillSlot bufferedSlot;
    private Vector3 bufferedTarget;
    private float bufferRemaining;
    private float bufferExpiresAt;
    private bool bufferedRelease;
    private const float InputBufferDuration = 0.18f;
    private SkillBase executingSkill;
    private readonly HashSet<SkillBase> executionSkills = new HashSet<SkillBase>();
    private bool restoreInterruptedControls;
    private bool previousMove, previousAttack, previousSkill;
    internal bool HasInterruptedControlRecovery => restoreInterruptedControls;

    internal void BeginSkillAction(SkillBase skill)
    {
        executingSkill = skill;
        executionSkills.Add(skill);
        restoreInterruptedControls = false;
        previousMove = _model.canMove;
        previousAttack = _model.canAttack;
        previousSkill = _model.canSkill;
    }

    internal bool OwnsRecoveryEvent(AnimationEvent animationEvent)
        => executingSkill != null && executingSkill.OwnsAnimationEvent(animationEvent);

    internal void CompleteSkillAction(AnimationEvent animationEvent)
    {
        if (OwnsRecoveryEvent(animationEvent)) executingSkill = null;
        // The final damage event may overlap recovery; retain the attack reference on normal completion.
    }

    internal void InterruptActiveSkill(bool recoverControls)
    {
        bufferedSkill = null;
        if (!recoverControls) restoreInterruptedControls = false;
        if (executingSkill != null)
        {
            SkillBase skill = executingSkill;
            executingSkill = null;
            skill.InterruptExecution();
            _model.PlayerController.InterruptAttackForSkill();
            _model.PlayerController.StopMove();
            if (!_model.isDie && !_model.Buff.isStun && _model.Anim != null &&
                _model.Anim.runtimeAnimatorController != null && _model.Anim.HasState(0, Animator.StringToHash("Base Layer.Idle")))
                _model.Anim.CrossFade("Base Layer.Idle", 0.1f);
            _model.canMove = false;
            _model.canAttack = false;
            _model.canSkill = false;
            restoreInterruptedControls = recoverControls;
        }
        foreach (SkillBase skill in executionSkills) skill.InterruptExecution();
        executionSkills.Clear();
        TryRecoverInterruptedControls();
    }

    internal void TryRecoverInterruptedControls()
    {
        if (!restoreInterruptedControls || !_model.CanRecoverSkillControls) return;
        restoreInterruptedControls = false;
        _model.canMove = previousMove;
        _model.canAttack = previousAttack;
        _model.canSkill = previousSkill;
    }

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
        if (_model.isDie || _model.Buff.isStun || _model.IsExternalControlLocked)
            InterruptActiveSkill(!_model.isDie && !_model.IsExternalControlLocked);
        else TryRecoverInterruptedControls();

        IdentitySkill?.UpdateSkill(deltaTime);
        DodgeSkill?.UpdateSkill(deltaTime);

        foreach (var skillPair in activeSkills)
        {
            if (skillPair.Value != null)
            {
                skillPair.Value.UpdateSkill(deltaTime);
            }
        }
        UpdateBufferedSkill(deltaTime);
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
        if (_model.isDie || _model.Buff.isStun || _model.IsExternalControlLocked)
        {
            bufferedSkill = null;
            return;
        }

        if (_model.Stigma != null && _model.Stigma.HasStigma(EStigmaType.Lv10_B))
        {
            if (slot == C_Enums.SkillSlot.W || slot == C_Enums.SkillSlot.E || slot == C_Enums.SkillSlot.R)
            {
                Debug.Log($"<color=red>[유아독존] {slot} 스킬은 봉인되어 사용할 수 없습니다.</color>");
                return;
            }
        }

        SkillBase targetSkill = GetSkillToSlot(slot);

        if (!_model.canSkill || !_model.canMove)
        {
            if (targetSkill != null && targetSkill.canUse)
            {
                bufferedSkill = targetSkill;
                bufferedSlot = slot;
                bufferedTarget = targetPos;
                bufferRemaining = InputBufferDuration;
                bufferExpiresAt = Time.time + InputBufferDuration;
                bufferedRelease = false;
            }
            return;
        }
        bufferedSkill = null;

        if (targetSkill != null)
        {
            if (targetSkill.UseSkill(targetPos))
            {
                if (slot == C_Enums.SkillSlot.Space)
                    _model.UseDodge();

                _model.TriggerSkillUsed();
            }
           
        }
        else
        {
            Debug.Log("해당 슬롯에 스킬 없음");
        }
    }

    public void ReleaseSkill(C_Enums.SkillSlot slot, Vector3 targetPos)
    {
        if (_model.isDie || _model.Buff.isStun || _model.IsExternalControlLocked)
        {
            InterruptActiveSkill(!_model.isDie && !_model.IsExternalControlLocked);
            return;
        }
        if (bufferedSkill != null && bufferedSlot == slot)
        {
            bufferedRelease = true;
            bufferedTarget = targetPos;
            return;
        }
        SkillBase targetSkill = GetSkillToSlot(slot);

        if (targetSkill != null)
        {
            targetSkill.ReleaseSkill(targetPos);
        }
    }

    private void UpdateBufferedSkill(float deltaTime)
    {
        if (bufferedSkill == null) return;
        bufferRemaining -= deltaTime;
        if (bufferRemaining <= 0f || Time.time >= bufferExpiresAt || _model.isDie || _model.Buff.isStun ||
            GetSkillToSlot(bufferedSlot) != bufferedSkill)
        {
            bufferedSkill = null;
            return;
        }
        if (!_model.canSkill || !_model.canMove) return;

        SkillBase skill = bufferedSkill;
        bool release = bufferedRelease;
        bufferedSkill = null;
        UseSkill(bufferedSlot, bufferedTarget);
        if (release && skill.isCharging) skill.ReleaseSkill(bufferedTarget);
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
        if (_model.Stat.Stat.remainSkillPoint < 1) return;

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
