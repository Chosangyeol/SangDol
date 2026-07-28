using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Skill_Space : SkillBase
{
    private NavMeshAgent _agent;

    public Skill_Space(CharacterModel model, SkillBaseSO skillData) : base(model, skillData)
    {
        _agent = model.GetComponent<NavMeshAgent>();
        return;
    }

    public override bool UseSkill(Vector3 targetPos)
    {
        if (canUse)
        {
            finalCoolTime = coolTime - _model.Stat.Stat.dodgeCooldownReduction;
            nowCoolTime = finalCoolTime;
            canUse = false;

            _model.PlayerController.StopMove();
            _model.PlayerController.FaceTo(targetPos);

            _model.Anim.SetTrigger("Skill_Space");

            _model.StartCoroutine(SkillActive(targetPos));
            Debug.Log("Space 스킬 사용!");
            return true;
        }

        Debug.Log("쿨타임 입니다.");
        return false;
    }

    IEnumerator SkillActive(Vector3 targetPos)
    {
        _model.canMove = false;

        float startTime = Time.time;
        float dashDuration = 0.2f; // 돌진 시간 (10거리 / 50속도 = 0.2초)

        _agent.ResetPath();
        _agent.velocity = Vector3.zero;

        Vector3 dashDirection = (targetPos - _model.transform.position).normalized;
        dashDirection.y = 0;

        float castRadius = 1f;
        int enemyLayer = LayerMask.GetMask("Enemy"); // ExecuteAttack의 LayerMask와 이름이 맞는지 확인하세요!

        // 0.2초 동안 무조건 루프가 돌아감
        while (Time.time < startTime + dashDuration)
        {
            float moveStep = 20f * Time.deltaTime;

            // 1. 앞에 몬스터가 있는지 확인 (break 제거)
            if (Physics.SphereCast(_model.transform.position, castRadius, dashDirection, out RaycastHit hit, moveStep, enemyLayer))
            {
                // 부딪히면 _agent.Move를 실행하지 않고 생략함 (이동 막힘 효과)
                // 만약 부딪혔을 때 살짝 밀려나는 효과나 이펙트를 추가하고 싶다면 이 안에 작성하면 됩니다.
            }
            else
            {
                // 2. 앞에 아무것도(몬스터가) 없을 때만 전진
                _agent.Move(dashDirection * moveStep);
            }

            yield return null;
        }
    }
}
