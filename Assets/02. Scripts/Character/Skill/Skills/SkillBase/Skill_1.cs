using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class Skill_1 : SkillBase
{
    private NavMeshAgent _agent;

    public Skill_1(CharacterModel model, SkillBaseSO skillData) : base(model,skillData)
    {
        _agent = model.GetComponent<NavMeshAgent>();
        return;
    }

    public override bool UseSkill(Vector3 targetPos)
    {
        if (_model.isDie || _model.Buff.isStun || _model.IsExternalControlLocked ||
            _agent == null || !_agent.isActiveAndEnabled || !_agent.isOnNavMesh) return false;
        if (base.UseSkill(targetPos))
        {
            BeginExecution();
            finalCoolTime = coolTime * _model.Stat.Stat.cooldownReduction.FinalValue;
            canUse = false;
            Debug.Log("skill_1 사용");

            _model.SetCantAttack();
            _model.SetCantMove();
            _model.SetCantSkill();

            _model.PlayerController.StopMove();
            _model.PlayerController.FaceTo(targetPos);

            _model.Anim.SetTrigger("Skill1");

            StartExecutionRoutine(SkillRoutine(targetPos));
            StartExecutionRoutine(Effect1(GetExecutionEffectPrefab(0),targetPos));
            return true;
        }
        Debug.Log("쿨타임 입니다.");
        return false;
    }

    IEnumerator SkillRoutine(Vector3 targetPos)
    {
        float startTime = Time.time;
        float dashDuration = 0.35f; // 돌진 시간 (10거리 / 50속도 = 0.2초)

        _agent.ResetPath();
        _agent.isStopped = true;
        _agent.velocity = Vector3.zero;

        Vector3 dashDirection = targetPos - _model.transform.position;
        dashDirection.y = 0;
        dashDirection.Normalize();

        float castRadius = _agent.radius + 0.3f;
        int enemyLayer = LayerMask.GetMask("Enemy"); // ExecuteAttack의 LayerMask와 이름이 맞는지 확인하세요!

        // 0.2초 동안 무조건 루프가 돌아감
        while (Time.time < startTime + dashDuration)
        {
            float moveStep = 35f * Time.deltaTime;

            // [수정 2] 내가 이번 프레임에 이동할 거리보다 '0.2f' 정도 더 앞을 미리 검사합니다.
            Vector3 nextPos = _model.transform.position + dashDirection * (moveStep + 0.2f);
            Vector3 checkPos = nextPos + Vector3.up * (_agent.height * 0.5f);

            // 더 크고 멀리 보는 구슬로 검사
            if (Physics.CheckSphere(checkPos, castRadius, enemyLayer))
            {
                // 범퍼에 먼저 닿았으므로 _agent.Move를 실행하지 않아, 미끄러지기 전에 제자리에 멈춤!
            }
            else
            {
                _agent.Move(dashDirection * moveStep);
            }

            yield return null;
        }

        yield return new WaitForSeconds(0.35f);

        StartExecutionRoutine(Effect2(GetExecutionEffectPrefab(1), _model.transform.position));

        yield return new WaitForSeconds(0.07f);

        StartExecutionRoutine(Effect3(GetExecutionEffectPrefab(2), _model.transform.position));
    }

    IEnumerator Effect1(PoolableMono prefab,Vector3 targetPos)
    {
        Vector3 dir = targetPos - _model.transform.position;
        
        PoolableMono effect = TakeExecutionEffect(prefab);
        if (effect == null) yield break;

        effect.transform.SetParent(_model.transform);

        effect.transform.localPosition = new Vector3(0, 0, 0.2f);
        effect.transform.localRotation = Quaternion.identity;

        yield return new WaitForSeconds(1f);

        effect.transform.SetParent(null);

        ReturnExecutionEffect(effect);
    }

    IEnumerator Effect2(PoolableMono prefab, Vector3 targetPos)
    {
        PoolableMono effect = TakeExecutionEffect(prefab);
        if (effect == null) yield break;

        Vector3 dir = _model.transform.forward;

        effect.transform.position = targetPos + _model.transform.forward * 0.5f;
        effect.transform.rotation = Quaternion.LookRotation(dir);

        yield return new WaitForSeconds(1f);

        ReturnExecutionEffect(effect);
    }

    IEnumerator Effect3(PoolableMono prefab, Vector3 targetPos)
    {
        PoolableMono effect = TakeExecutionEffect(prefab);
        if (effect == null) yield break;

        Vector3 dir = _model.transform.forward;

        effect.transform.position = targetPos + _model.transform.forward * 1.5f;
        effect.transform.rotation = Quaternion.LookRotation(dir);

        yield return new WaitForSeconds(1f);

        ReturnExecutionEffect(effect);
    }

}
