using UnityEngine;

public class PatrolState : State
{
    public PatrolState(EnemyModel owner) : base(owner)
    {

    }

    public override void EnterState()
    {
        Debug.Log("순찰 상태로 진입");
    }

    public override void UpdateState()
    {
        Debug.Log("순찰 상태 업데이트");
    }

    public override void ExitState()
    {
        Debug.Log("순찰 상태에서 벗어남");
    }
}
