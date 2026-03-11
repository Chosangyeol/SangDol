using UnityEngine;

public class IdleState : State
{
    public IdleState(EnemyModel owner) : base(owner)
    {

    }

    public override void EnterState()
    {
        Debug.Log("대기 상태로 진입");
    }

    public override void UpdateState()
    {
        Debug.Log("대기 상태 업데이트");
    }

    public override void ExitState()
    {
        Debug.Log("대기 상태에서 벗어남");
    }
}
