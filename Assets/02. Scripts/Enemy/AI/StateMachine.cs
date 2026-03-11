using UnityEngine;

public class StateMachine
{
    // 몬스터의 현재 상태
    public State CurState { get; set; }
     
    // 몬스터 자신
    private EnemyModel _owner;

    // FSM 생성자
    public StateMachine(EnemyModel owner)
    {
        _owner = owner;
        return;
    }

    /// <summary>
    /// 해당 몬스터의 현재 상태(IState)를 다른 상태로 전환할 때 호출되는 메서드
    /// </summary>
    /// <param name="newState">전환 할 상태</param>
    public void ChangeState(State newState)
    {
        if (newState == null)
            return;

        if (CurState == newState)
            return;

        CurState?.ExitState();
        CurState = newState;
        CurState?.EnterState();

    }

    /// <summary>
    /// 해당 몬스터의 현재 상태의 매 프레임마다 호출되는 메서드를 호출하는 메서드
    /// </summary>
    public void UpdateState()
    {
        CurState?.UpdateState();
    }
}
