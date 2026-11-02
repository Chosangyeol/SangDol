using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_Z : SkillBase
{
    public Skill_Z(CharacterModel model, SkillBaseSO skillData) : base(model,skillData)
    {
        return;
    }

    public override bool UseSkill(Vector3 targetPos)
    {
        if (!(skillData is Skill_ZSO zSO) ||
            _model.Stat.Stat.idenCurrent < _model.Stat.Stat.idenMax ||
            !canUse || !base.UseSkill(targetPos)) return false;

        _model.ResetIden();
        SBuff sBuff = new SBuff(
            _model.gameObject,
            _model.gameObject,
            new IdenBuff(_model, zSO.buffSO, 5f));
        _model.Buff.AddBuff(sBuff);
        return true;
    }
}
