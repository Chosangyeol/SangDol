using System.Collections;
using System;
using UnityEngine;

public class Skill_1 : SkillBase
{
    public Skill_1(CharacterModel model, SkillBaseSO skillData) : base(model,skillData)
    {
        return;
    }

    public override bool UseSkill(Vector3 targetPos)
    {
        if (canUse)
        {
            // 시간 내에 재시전 가능한 스킬이라, 추가로 조건 달아야할듯?
            nowCoolTime = coolTime;
            canUse = false;
            Debug.Log("skill_1 사용");

            //Start(Effect(skillData.skillEffects[0]));
            _model.SkillCorutaine(Effect(skillData.skillEffects[0],targetPos));
            return true;
        }
        Debug.Log("쿨타임 입니다.");
        return false;
    }

    IEnumerator Effect(PoolableMono prefab,Vector3 targetPos)
    {
        Vector3 dir = targetPos - _model.transform.position;

        PoolableMono effect = PoolManager.Instance.Pop(prefab.name);
        effect.transform.position = _model.transform.position;
        effect.transform.rotation = Quaternion.LookRotation(dir);

        yield return new WaitForSeconds(3f);
        
        PoolManager.Instance.Push(effect);
    }
    
}
