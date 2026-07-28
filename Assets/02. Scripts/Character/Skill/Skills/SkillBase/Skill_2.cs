using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Skill_2 : SkillBase
{
    private NavMeshAgent _agent;
    private float _maxRange = 20f;

    public Skill_2(CharacterModel model, SkillBaseSO skillData) : base(model, skillData)
    {
        _agent = model.GetComponent<NavMeshAgent>();
        return;
    }

    public override bool UseSkill(Vector3 targetPos)
    {
        if (base.UseSkill(targetPos))
        {
            nowCoolTime = coolTime;
            canUse = false;

            _model.PlayerController.StopMove();
            _model.PlayerController.FaceTo(targetPos);

            Vector3 startPos = _model.transform.position;

            _model.SetCantAttack();
            _model.SetCantMove();
            _model.SetCantSkill();

            // 나와 목표 지점 사이의 방향과 거리를 구합니다 (Y축 높이 차이는 무시)
            Vector3 dir = targetPos - startPos;
            dir.y = 0;

            if (dir.magnitude > _maxRange)
            {
                // 내 위치에서 클릭한 '방향(normalized)'으로 '최대 사거리(10f)'만큼만 간 좌표로 덮어씌움
                targetPos = startPos + dir.normalized * _maxRange;

                targetPos.y = startPos.y; 
            }
            // ==========================================

            _model.StartCoroutine(JumpRoutine(targetPos));

            return true;
        }

        return false;
    }

    IEnumerator JumpRoutine(Vector3 targetPos)
    {
        // 1. 점프 설정
        float jumpDuration = 0.9f; // 거리에 상관없이 무조건 0.9초 걸림
        float jumpHeight = 5.0f;   // 점프하는 최대 높이
        float elapsedTime = 0f;

        Vector3 startPos = _model.transform.position;

        // 점프 방향 바라보기
        Vector3 lookDir = (targetPos - startPos).normalized;
        lookDir.y = 0;
        if (lookDir != Vector3.zero)
        {
            _model.transform.rotation = Quaternion.LookRotation(lookDir);
        }

        // 2. 공중에 뜨기 위해 NavMeshAgent를 잠시 끕니다. (안 끄면 바닥을 쓸고 감)
        _agent.enabled = false;
        
        _model.canMove = false;

        _model.Anim.SetTrigger("Skill2");

        // 3. 고정 시간 동안 포물선 이동
        while (elapsedTime < jumpDuration)
        {
            elapsedTime += Time.deltaTime;

            // t는 0에서 시작해서 시간이 다 되면 1이 됩니다. (진행률)
            float t = elapsedTime / jumpDuration;

            // 직선으로 시작점부터 끝점까지 이동하는 위치
            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, t);

            // [핵심] Y축(높이)에 포물선 추가
            // t가 0일때 Sin(0)=0, t가 0.5일때 Sin(PI/2)=1(최고점), t가 1일때 Sin(PI)=0
            currentPos.y += Mathf.Sin(t * Mathf.PI) * jumpHeight;

            // 캐릭터 위치 적용
            _model.transform.position = currentPos;

            yield return null;
        }

        // 4. 도착 후 오차 보정 및 지면 착지
        _model.transform.position = targetPos;
        _agent.enabled = true; // NavMeshAgent 다시 켜기
        // 5. 착지(강타) 이펙트와 데미지 판정
        _model.StartCoroutine(Effect(skillData.skillEffects[0], targetPos));
    }

    IEnumerator Effect(PoolableMono prefab, Vector3 targetPos)
    {
        Vector3 dir = targetPos - _model.transform.position;

        PoolableMono effect = PoolManager.Instance.Pop(prefab.name);
        effect.transform.position = _model.transform.position;
        effect.transform.rotation = Quaternion.LookRotation(dir);

        yield return new WaitForSeconds(2f);

        PoolManager.Instance.Push(effect);
    }
}
