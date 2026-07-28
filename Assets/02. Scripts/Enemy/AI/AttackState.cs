using System.Threading;
using UnityEngine;

public class AttackState : State
{
    private float _attackTimer = 0f;
    public bool changeState = false;

    private float _rotationSpeed = 8f;
    private bool _isRotationComplete = false;
    public AttackState(EnemyModel owner) : base(owner)
    {

    }

    public override void EnterState()
    {
        _owner.Agent.isStopped = true;
        _owner.Agent.velocity = Vector3.zero;
        _owner.canAttack = false;
        changeState = false;
        _isRotationComplete = false;
        _attackTimer = _owner.Stat.attackSpeed;
        Debug.Log("공격 상태로 진입");
    }

    public override void UpdateState()
    {
        if (_owner.Target == null) return;

        if (!_isRotationComplete)
        {
            Vector3 direction = _owner.Target.transform.position - _owner.transform.position;
            direction.y = 0f; // 수평으로만 회전

            if (direction != Vector3.zero)
            {
                // 목표 방향으로 부드럽게 회전
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                _owner.transform.rotation = Quaternion.Slerp(
                    _owner.transform.rotation,
                    targetRotation,
                    Time.deltaTime * _rotationSpeed
                );

                float angle = Vector3.Angle(_owner.transform.forward, direction);


                if (angle <= 5f)
                {
                    _isRotationComplete = true; // 회전 완료 선언!
                    _owner.Anim.SetTrigger("Attack");
                    Debug.Log("조준 완료 -> 공격 애니메이션 재생");
                }
            }
            return;
        }

        if (!_owner.canAttack) return;

        _attackTimer -= Time.deltaTime;

        if (_attackTimer <= 0f)
        {
            if (_owner.Target != null)
            {
                changeState = true;
                return;
            }
        }
        Debug.Log("공격 상태 업데이트");
    }

    public override void ExitState()
    {
        _owner.Agent.isStopped = false;
        Debug.Log("공격 상태에서 벗어남");
    }

}
