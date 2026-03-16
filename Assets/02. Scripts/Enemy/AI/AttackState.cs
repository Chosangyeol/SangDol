using UnityEngine;

public class AttackState : State
{
    private bool _canAttack = false;
    private float _attackTimer = 0f;
    public bool CanAttack => _canAttack;
    public AttackState(EnemyModel owner) : base(owner)
    {

    }

    public override void EnterState()
    {
        _owner.Agent.isStopped = true;
        _owner.Agent.velocity = Vector3.zero;

        _attackTimer = 0f;
        _canAttack = false;
        // 추후 애니메이션에서 이벤트로 공격 실행하게 수정할 예정
        _owner.Attack();
        Debug.Log("공격 상태로 진입");
    }

    public override void UpdateState()
    {
        _attackTimer += Time.deltaTime;

        if (_attackTimer >= _owner.Stat.attackSpeed)
            _canAttack = true;

        Debug.Log("공격 상태 업데이트");
    }

    public override void ExitState()
    {
        _owner.Agent.isStopped = false;

        _attackTimer = 0f;
        _canAttack = false;
        Debug.Log("공격 상태에서 벗어남");
    }

}
