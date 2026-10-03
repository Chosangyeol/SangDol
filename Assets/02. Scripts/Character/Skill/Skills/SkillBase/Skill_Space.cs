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
        if (_model.isDie || _model.Buff.isStun || _agent == null ||
            !_agent.isActiveAndEnabled || !_agent.isOnNavMesh) return false;
        if (canUse)
        {
            finalCoolTime = coolTime - _model.Stat.Stat.dodgeCooldownReduction;
            nowCoolTime = finalCoolTime;
            canUse = false;

            _model.PlayerController.StopMove();
            _model.PlayerController.InterruptAttackForSkill();
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
        bool previousAttack = _model.canAttack;
        bool previousSkill = _model.canSkill;
        _model.canMove = false;
        _model.canAttack = false;
        _model.canSkill = false;

        float startTime = Time.time;
        float dashDuration = 0.2f; // 돌진 시간 (10거리 / 50속도 = 0.2초)

        _agent.ResetPath();
        _agent.isStopped = true;
        _agent.velocity = Vector3.zero;

        Vector3 dashDirection = targetPos - _model.transform.position;
        dashDirection.y = 0;
        dashDirection.Normalize();

        float castRadius = 1f;
        int enemyLayer = LayerMask.GetMask("Enemy"); // ExecuteAttack의 LayerMask와 이름이 맞는지 확인하세요!

        try
        {
            while (Time.time < startTime + dashDuration && !_model.isDie && !_model.Buff.isStun &&
                _agent.isActiveAndEnabled && _agent.isOnNavMesh)
            {
                float moveStep = 20f * Time.deltaTime;
                if (!Physics.SphereCast(_model.transform.position, castRadius, dashDirection,
                    out RaycastHit hit, moveStep, enemyLayer))
                    _agent.Move(dashDirection * moveStep);

                yield return null;
            }
            // Match the existing dash animation's recovery window without relying on its event.
            while (Time.time < startTime + 1f / 3f && !_model.isDie && !_model.Buff.isStun)
                yield return null;
        }
        finally
        {
            if (_model != null && !_model.isDie)
            {
                _model.canMove = true;
                _model.canAttack = previousAttack;
                _model.canSkill = previousSkill;
            }
        }
    }
}
