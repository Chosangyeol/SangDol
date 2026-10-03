using System.Collections;
using UnityEngine;

public class Skill_4 : ChargeSkillBase
{
    private float tickTimer = 0f;
    private const float TICK_INTERVAL = 0.5f; // 0.5초마다 다단히트

    private PoolableMono spinEffect;

    public Skill_4(CharacterModel model, SkillBaseSO skillData) : base(model, skillData)
    {
        return;
    }

    public override bool UseSkill(Vector3 targetPos)
    {
        if (_model.isDie || _model.Buff.isStun || _model.IsExternalControlLocked) return false;
        if (base.UseSkill(targetPos))
        {
            BeginExecution();
            spinEffect = null;
            isCharging = true;
            
            currentChargeTime = 0f;
            tickTimer = 0f;

            _model.canMove = false;
            _model.canAttack = false;
            _model.canSkill = false;

            _model.Anim.SetTrigger("Skill4_Spin");
            StartExecutionRoutine(Effect(GetExecutionEffectPrefab(0), _model.transform.position));
            return true;
        }
        return false;
    }

    public override void UpdateSkill(float deltaTime)
    {
        base.UpdateSkill(deltaTime);

        if (isCharging)
        {
            tickTimer += deltaTime;
            if (tickTimer > TICK_INTERVAL)
            {
                tickTimer -= TICK_INTERVAL;
                ExecuteHoldingTick();
            }
        }
    }

    private void ExecuteHoldingTick()
    {
        int enemyLayer = LayerMask.GetMask("Enemy");
        Collider[] hitEnemies = Physics.OverlapSphere(_model.transform.position, 5f, enemyLayer);

        foreach (Collider target in hitEnemies)
        {
            EnemyBase enemy = target.GetComponentInParent<EnemyBase>();
            if (enemy != null)
            {
                float damageBase = _model.Stat.Stat.attackDamage.FinalValue * 2.0f;

                if (_model.Stigma != null && _model.Stigma.HasStigma(EStigmaType.Lv8_B))
                {
                    // 현재 체력 비율 계산 (0 ~ 1 사이)
                    float hpPercent = (float)_model.Stat.Stat.curHp / _model.Stat.Stat.maxHp.FinalValue;
                    if (hpPercent <= 0.3f)
                    {
                        damageBase *= 1.3f; // 주는 피해 30% 증가
                    }
                }

                // 2. 다단히트 데미지 (공격력의 200% = 2.0f)
                SDamageInfo damageInfo = new SDamageInfo
                {
                    damage = damageBase,
                    source = _model.gameObject,
                    knockDownPower = 0, // 홀딩 중에는 넘어뜨리지 않음
                    isCounterable = false,
                    isCritical = _model.GetCritical(),
                    isHeadattack = false,
                    isBackattack = false
                };

                enemy.Damaged(damageInfo);
            }
        }

    }

    public override void ReleaseSkill(Vector3 targetPos)
    {
        if (!isCharging || !CanContinueExecution) return;

        CheckPerfectCharge();

        if (isCharging)
        {
            GameEvent.OnGaugeUpdate?.Invoke(false, "", 0f, -1f, -1f);
        }

        isCharging = false;
        finalCoolTime = coolTime * _model.Stat.Stat.cooldownReduction.FinalValue;
        nowCoolTime = finalCoolTime;

        if (isPerfectCharge)
        {
            Debug.Log("<color=green>퍼펙트 존 성공! [심판의 일격] 발동!</color>");
            _model.canMove = false;
            _model.PlayerController.StopMove();
            _model.Anim.SetTrigger("Skill4_Perfect");
            if (AudioManager.instance != null) AudioManager.instance.PlaySFX(C_Enums.SFX_List.Player_Skill4);
            StartExecutionRoutine(Effect2(GetExecutionEffectPrefab(1), _model.transform.position));
            if (spinEffect != null)
            {
                ReturnExecutionEffect(spinEffect);
                spinEffect = null;
            }
            
        }
        else
        {
            _model.PlayerController.StopMove();
            Debug.Log("퍼펙트 실패. 일반 회전 베기로 종료.");
            _model.Anim.SetTrigger("Skill4_End");
            if (spinEffect != null)
            {
                ReturnExecutionEffect(spinEffect);
                spinEffect = null;
            }
            // 덜 모았거나 지나쳤을 때 나가는 약한 피니시 애니메이션
            //_model.Anim.SetTrigger("SkillR_NormalEnd");
        }
    }

    IEnumerator Effect(PoolableMono prefab, Vector3 targetPos)
    {
        PoolableMono effect = TakeExecutionEffect(prefab);
        if (effect == null) yield break;
        spinEffect = effect;

        Vector3 dir = _model.transform.forward;

        effect.transform.SetParent(_model.transform);

        effect.transform.position = targetPos;
        effect.transform.rotation = Quaternion.LookRotation(dir);
        yield return new WaitForSeconds(3f);

        spinEffect = null;
        ReturnExecutionEffect(effect);
    }

    IEnumerator Effect2(PoolableMono prefab, Vector3 targetPos)
    {
        yield return new WaitForSeconds(1.3f);

        PoolableMono effect = TakeExecutionEffect(prefab);
        if (effect == null) yield break;

        Vector3 dir = _model.transform.forward;

        effect.transform.position = targetPos;
        effect.transform.rotation = Quaternion.LookRotation(dir);
        yield return new WaitForSeconds(1f);
        ReturnExecutionEffect(effect);
    }
}
