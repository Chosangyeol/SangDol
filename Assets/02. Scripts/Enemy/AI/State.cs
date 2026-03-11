using UnityEngine;

public abstract class State
{
    protected EnemyModel _owner; // 상태를 소유한 몬스터

    /// <summary>
    /// 해당 상태를 초기화하는 생성자
    /// </summary>
    /// <param name="owner"></param>
    protected State(EnemyModel owner)
    {
        _owner = owner;
    }

    // 해당 상태가 시작될 때 호출되는 메서드
    // 해당 메서드는 특정 상태에 대한 초기 설정을 담당함
    public abstract void EnterState();

    // 해당 상태 중 매 프레임마다 호출되는 메서드
    // 해당 메서드는 특정 상태가 활성화된 상태일 때 실행되는 로직을 담당함
    public abstract void UpdateState();

    // 해당 상태가 종료될 때 호출되는 메서드
    // 해당 메서드는 특정 상태를 벗어날 때 상태 정리 혹은 상태 전환을 담당함
    public abstract void ExitState();
}
