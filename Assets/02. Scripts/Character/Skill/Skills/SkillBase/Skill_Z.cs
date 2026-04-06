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
        if (canUse && _model.Stat.Stat.idenCurrent == 100)
        {
            if (skillData is Skill_ZSO zSO)
            {
                SBuff sBuff = new SBuff(
                    _model.gameObject,
                    _model.gameObject,
                    new StatBuff(_model, zSO.buffSO, 5f, C_Enums.CharacterStat.AttackDamage, false, 0.5f)
                    );
                
                _model.Buff.AddBuff(sBuff);
                _model.ResetIden();

                GameEvent.OnStatChange?.Invoke(_model.Stat.Stat);
                return true;
            }
        }
        Debug.Log("쿨타임 입니다.");
        return false;
    }
}
