using UnityEngine;
using UnityEngine.Rendering;

public class DieState : State
{
    private float destroyDelay = 10f;
    private float timer = 0f;

    public DieState(EnemyModel owner) : base(owner)
    {

    }

    public override void EnterState()
    {
        timer = 0f;
        _owner.Agent.isStopped = true;
        _owner.Agent.velocity = Vector3.zero;
        Debug.Log("사망 애니메이션 실행");
    }

    public override void UpdateState()
    {
        timer += Time.deltaTime;

        if (timer >= destroyDelay)
            Debug.Log("몬스터 시체 삭제 ( 풀에 Push )");
    }

    public override void ExitState()
    {
        Debug.Log("사망 상태에서 벗어남");
    }

}
