using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_Space : SkillBase
{
    public Skill_Space(CharacterModel model, SkillBaseSO skillData) : base(model, skillData)
    {
        return;
    }

    public override bool UseSkill(Vector3 targetPos)
    {
        if (canUse)
        {
            finalCoolTime = coolTime - _model.Stat.Stat.dodgeCooldownReduction;
            nowCoolTime = finalCoolTime;
            canUse = false;

            _model.canMove = false;
            _model.Anim.SetTrigger("Skill_Space");
            // 스킬 사용 로직 구현
            Debug.Log("Space 스킬 사용!");
            return true;
        }

        Debug.Log("쿨타임 입니다.");
        return false;
    }
}
