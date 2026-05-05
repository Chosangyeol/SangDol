using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargeSkillBase : SkillBase
{
    public ChargeSkillBase(CharacterModel model, SkillBaseSO skillData) : base(model, skillData) { }

    public override bool UseSkill(Vector3 targetPos)
    {
        if (base.UseSkill(targetPos))
        {
            canUse = false;
            isCharging = true;
            currentChargeTime = 0f;

            return true;
        }
        return false;
    }

    public override void ReleaseSkill(Vector3 targetPos)
    {      
        if (!isCharging) return;

        if (isCharging)
        {
            GameEvent.OnGaugeUpdate?.Invoke(false, "", 0f, -1f, -1f);
        }

        isCharging = false;
        finalCoolTime = coolTime * _model.Stat.Stat.cooldownReduction.FinalValue;
        nowCoolTime = finalCoolTime;


    }
}