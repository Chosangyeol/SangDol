using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackContainer : MonoBehaviour
{
    private CharacterModel _model;

    // [핵심] 현재 실행 중인 스킬을 담아둘 그릇
    public SkillBase currentSkill;

    public SkillBaseSO previewSkillData;
    private void Awake()
    {
        _model = GetComponent<CharacterModel>();
    }


    public void AnimEvent_ExevuteSkillAttack()
    {
        if (currentSkill == null || currentSkill.skillData == null) return;

        SkillBaseSO data = currentSkill.skillData;

        int enemyLayer = LayerMask.GetMask("Enemy");

        Vector3 attackPos = _model.transform.position + _model.transform.forward * data.forwardOffset;

        float scaleMultiplier = currentSkill.GetCurrentScaleMultiplier();

        Collider[] hitEnemies = null; // 판정된 적들을 담을 배열

        // [핵심] 스킬 모양에 따라 다른 Overlap 함수 호출
        switch (data.attackShape)
        {
            case EAttackShape.Sphere:
                // 반지름(Radius)에 배율 곱하기
                hitEnemies = Physics.OverlapSphere(attackPos, data.attackRadius * scaleMultiplier, enemyLayer);
                break;

            case EAttackShape.Box:
                // 박스 크기(boxSize)에 배율 곱하기
                Vector3 scaledBoxSize = data.boxSize * scaleMultiplier;
                hitEnemies = Physics.OverlapBox(attackPos, scaledBoxSize * 0.5f, _model.transform.rotation, enemyLayer);
                break;
        }

        // 아무도 안 맞았으면 종료
        if (hitEnemies == null || hitEnemies.Length == 0) return;

        foreach (Collider target in hitEnemies)
        {
            EnemyBase enemy = target.GetComponentInParent<EnemyBase>();

            if (enemy != null)
            {
                float skillMultiplier = currentSkill.GetCurrentDamageMultiplier();
                float chargeMultiplier = currentSkill.GetChargeMultiplier();

                float damageBase = _model.Stat.Stat.attackDamage.FinalValue * skillMultiplier * chargeMultiplier;

                if (_model.Stigma != null && _model.Stigma.HasStigma(EStigmaType.Lv8_B))
                {
                    // 현재 체력 비율 계산 (0 ~ 1 사이)
                    float hpPercent = (float)_model.Stat.Stat.curHp / _model.Stat.Stat.maxHp.FinalValue;
                    if (hpPercent <= 0.3f)
                    {
                        damageBase *= 1.3f; // 주는 피해 30% 증가
                    }
                }

                SDamageInfo damageInfo = new SDamageInfo
                {
                    damage = damageBase,
                    source = _model.gameObject,
                    knockDownPower = data.knockDownPower,
                    isCounterable = false,
                    isCritical = _model.GetCritical(),
                    isHeadattack = false,
                    isBackattack = false
                };

                enemy.Damaged(damageInfo);
                Debug.Log($"{target.name}에게 {data.attackShape} 형태의 타격 적중!");
            }
        }
    }

    private void OnDrawGizmos()
    {
        // 1. 그릴 데이터 결정 (게임 실행 중엔 currentSkill, 아니면 previewSkillData 사용)
        SkillBaseSO dataToDraw = null;

        if (Application.isPlaying && currentSkill != null && currentSkill.skillData != null)
        {
            dataToDraw = currentSkill.skillData;
        }
        else if (previewSkillData != null)
        {
            dataToDraw = previewSkillData;
        }

        // 그릴 데이터가 없으면 종료
        if (dataToDraw == null) return;

        float scaleMultiplier = 1f;

        // 현재 실행 중인 스킬이 있다면 배율 가져오기
        if (Application.isPlaying && currentSkill != null)
        {
            scaleMultiplier = currentSkill.GetCurrentScaleMultiplier();
        }

        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        Vector3 attackPos = transform.position + transform.forward * dataToDraw.forwardOffset;

        switch (dataToDraw.attackShape)
        {
            case EAttackShape.Sphere:
                Gizmos.DrawWireSphere(attackPos, dataToDraw.attackRadius * scaleMultiplier);
                break;

            case EAttackShape.Box:
                Gizmos.matrix = Matrix4x4.TRS(attackPos, transform.rotation, Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, dataToDraw.boxSize * scaleMultiplier);
                Gizmos.matrix = Matrix4x4.identity;
                break;
        }
    }
}
