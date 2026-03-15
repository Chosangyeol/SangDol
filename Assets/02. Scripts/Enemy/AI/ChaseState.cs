using UnityEngine;

public class ChaseState : State
{
    public ChaseState(EnemyModel owner) : base(owner)
    {

    }

    public override void EnterState()
    {
        
        Debug.Log("추적 상태로 진입");
    }

    public override void UpdateState()
    {
        _owner.Agent.SetDestination(_owner.Target.transform.position);
        Debug.Log("추적 상태 업데이트");
    }

    public override void ExitState()
    {
        Debug.Log("추적 상태에서 벗어남");
    }
}
