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
        if (_model.isDie || _model.Buff.isStun || _agent == null ||
            !_agent.isActiveAndEnabled || !_agent.isOnNavMesh) return false;

        Vector3 startPos = _model.transform.position;
        Vector3 direction = targetPos - startPos;
        direction.y = 0f;
        if (direction.magnitude > _maxRange)
            targetPos = startPos + direction.normalized * _maxRange;

        // Validate the landing before consuming the cooldown or disabling navigation.
        if (!NavMesh.SamplePosition(targetPos, out NavMeshHit landing, 1f, _agent.areaMask))
            return false;
        if (!base.UseSkill(landing.position)) return false;

        _model.SetCantAttack();
        _model.SetCantMove();
        _model.SetCantSkill();
        _model.StartCoroutine(JumpRoutine(landing.position));
        return true;
    }

    IEnumerator JumpRoutine(Vector3 targetPos)
    {
        const float jumpDuration = 0.9f;
        const float jumpHeight = 5f;
        float elapsedTime = 0f;
        Vector3 startPos = _model.transform.position;
        bool landed = false;
        _agent.enabled = false;
        _model.Anim.SetTrigger("Skill2");

        try
        {
            while (elapsedTime < jumpDuration && !_model.isDie && !_model.Buff.isStun)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / jumpDuration);
                Vector3 position = Vector3.Lerp(startPos, targetPos, t);
                position.y += Mathf.Sin(t * Mathf.PI) * jumpHeight;
                _model.transform.position = position;
                yield return null;
            }
            landed = !_model.isDie && !_model.Buff.isStun;
        }
        finally
        {
            if (_model != null && _agent != null)
            {
                // Interrupted jumps return to a known valid point instead of enabling an agent in mid-air.
                _model.transform.position = landed ? targetPos : startPos;
                if (!_model.isDie)
                {
                    _agent.enabled = true;
                    if (_agent.isOnNavMesh) _agent.Warp(_model.transform.position);
                    _model.SetCanAttack();
                    _model.SetCanMove();
                    _model.SetCanSkill();
                }
            }
        }
        if (landed && skillData.skillEffects != null && skillData.skillEffects.Length > 0 && skillData.skillEffects[0] != null)
            _model.StartCoroutine(Effect(skillData.skillEffects[0], targetPos));
    }
    IEnumerator Effect(PoolableMono prefab, Vector3 targetPos)
    {
        Vector3 dir = _model.transform.forward;

        PoolableMono effect = PoolManager.Instance.Pop(prefab.name);
        effect.transform.position = _model.transform.position;
        effect.transform.rotation = Quaternion.LookRotation(dir);

        yield return new WaitForSeconds(2f);

        PoolManager.Instance.Push(effect);
    }
}
