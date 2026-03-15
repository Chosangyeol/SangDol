using UnityEngine;

public class ReturnState : State
{
    public bool isReturning = false;

    public ReturnState(EnemyModel owner) : base(owner)
    {

    }

    public override void EnterState()
    {
        isReturning = true;
        _owner.Agent.speed = 10f;
        Debug.Log("복귀 상태로 진입");
    }

    public override void UpdateState()
    {
        _owner.Agent.SetDestination(_owner.SpawnPoint.position);

        if (Vector3.Distance(_owner.transform.position, _owner.SpawnPoint.position) < 0.1f)
        {
            isReturning = false;
            _owner.Agent.isStopped = true;
            _owner.Agent.velocity = Vector3.zero;
        }
        Debug.Log("복귀 상태 업데이트");
    }

    public override void ExitState()
    {
        _owner.Agent.isStopped = false;
        _owner.Agent.speed = _owner.Stat.moveSpeed;
        Debug.Log("복귀 상태에서 벗어남");
    }
}
