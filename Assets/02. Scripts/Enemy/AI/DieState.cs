using UnityEngine;
using UnityEngine.Rendering;

public class DieState : State
{
    private float destroyDelay = 5f;
    private float timer = 0f;
    private bool isPushed = false;

    public DieState(EnemyModel owner) : base(owner)
    {

    }

    public override void EnterState()
    {
        timer = 0f;
        isPushed = false;

        _owner.Agent.isStopped = true;
        _owner.Agent.velocity = Vector3.zero;
        _owner.Agent.enabled = false;

        Debug.Log("사망 애니메이션 실행");
    }

    public override void UpdateState()
    {
        if (isPushed) return;

        timer += Time.deltaTime;
        if (timer >= destroyDelay)
        {
            isPushed = true;
            _owner.OnReturnToPool?.Invoke(_owner);
            PoolManager.Instance.Push(_owner);
        }
    }

    public override void ExitState()
    {
        Debug.Log("사망 상태에서 벗어남");
    }

}
