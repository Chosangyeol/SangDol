using UnityEngine;

public class ReturnState : State
{
    public ReturnState(EnemyModel owner) : base(owner)
    {

    }

    public override void EnterState()
    {
        Debug.Log("복귀 상태로 진입");
    }

    public override void UpdateState()
    {
        Debug.Log("복귀 상태 업데이트");
    }

    public override void ExitState()
    {
        Debug.Log("복귀 상태에서 벗어남");
    }
}
