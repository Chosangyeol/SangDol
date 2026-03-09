using System.Collections;
using UnityEngine;

public class Skill_2 : SkillBase
{
    private bool canChain = false;

    public Skill_2(CharacterModel model, SkillBaseSO skillData) : base(model, skillData)
    {
        return;
    }

    public override bool UseSkill(Vector3 targetPos)
    {
        if (canChain)
        {
            Debug.Log("skill_2-2 사용");

            // 체인 스킬 사용
            nowCoolTime = coolTime;
            canChain = false;
            canUse = false;

            _model.SkillCorutaine(Effect(skillData.skillEffects[1], targetPos));

            return true;
        }

        if (canUse)
        {
            // 시간 내에 재시전 가능한 스킬이라, 추가로 조건 달아야할듯?
            Debug.Log("skill_2-1 사용");

            canChain = true;

            _model.SkillCorutaine(ChainOn());
            _model.SkillCorutaine(Effect(skillData.skillEffects[0], targetPos));

            return true;
        }

        Debug.Log("쿨타임 입니다.");

        return false;
    }

    IEnumerator ChainOn()
    {
        yield return new WaitForSeconds(5f);

        if (canChain)
        {
            nowCoolTime = coolTime;
            canChain = false;
            canUse = false;
            Debug.Log("체인 스킬 사용 기회 종료");
        }
    }

    IEnumerator Effect(PoolableMono prefab, Vector3 targetPos)
    {
        Vector3 dir = targetPos - _model.transform.position;

        PoolableMono effect = PoolManager.Instance.Pop(prefab.name);
        effect.transform.position = _model.transform.position;
        effect.transform.rotation = Quaternion.LookRotation(dir);

        yield return new WaitForSeconds(3f);

        PoolManager.Instance.Push(effect);
    }
}
