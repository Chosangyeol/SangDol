using System.Collections;
using UnityEngine;

public class Skill_3 : ChargeSkillBase
{
    public Skill_3(CharacterModel model, SkillBaseSO skillData) : base(model, skillData)
    {
        return;
    }

    public override bool UseSkill(Vector3 targetPos)
    {
        if (base.UseSkill(targetPos))
        {
            isCharging = true;
            currentChargeTime = 0f;

            _model.SetCantAttack();
            _model.SetCantMove();
            _model.SetCantSkill();

            Debug.Log("스킬 3 차지 시작");
            _model.Anim.SetTrigger("Skill3_Charge");
            AudioManager.instance.PlaySFX(C_Enums.SFX_List.Player_Skill3_Charge);

            return true;
        }
        return false;
    }

    public override void ReleaseSkill(Vector3 targetPos)
    {
        if (!isCharging) return;

        float finalScaleMultiplier = GetCurrentScaleMultiplier();

        AudioManager.instance.PlaySFX(C_Enums.SFX_List.Player_Skill3_Impact);

        if (isCharging)
        {
            GameEvent.OnGaugeUpdate?.Invoke(false, "", 0f,-1,-1);
        }

        isCharging = false;
        finalCoolTime = coolTime * _model.Stat.Stat.cooldownReduction.FinalValue;
        nowCoolTime = finalCoolTime;

        Debug.Log("스킬 3 차지 해제, 타격 발동!");

        _model.Anim.SetTrigger("Skill3_ChargeEnd");
        _model.StartCoroutine(Effect(skillData.skillEffects[0],_model.transform.position, finalScaleMultiplier));
    }

    IEnumerator Effect(PoolableMono prefab, Vector3 targetPos,float scaleMulti)
    {
        yield return new WaitForSeconds(0.2f);

        PoolableMono effect = PoolManager.Instance.Pop(prefab.name);

        Vector3 dir = _model.transform.forward;

        effect.transform.position = targetPos;
        effect.transform.rotation = Quaternion.LookRotation(dir);

        effect.transform.localScale = prefab.transform.localScale * scaleMulti;

        yield return new WaitForSeconds(1f);

        effect.transform.localScale = prefab.transform.localScale;

        PoolManager.Instance.Push(effect);
    }
}
