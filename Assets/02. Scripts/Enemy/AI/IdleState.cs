using UnityEngine;

public class IdleState : State
{
    public float patrolDelay = 3f;
    public bool canPatrol = false;

    public IdleState(EnemyModel owner) : base(owner)
    {
        patrolDelay = 3f;
        canPatrol = false;
    }

    public override void EnterState()
    {
        Debug.Log("대기 상태로 진입");
        _owner.Anim.SetBool("Patrol", false);
        _owner.Anim.SetBool("InBattle", false);
    }

    public override void UpdateState()
    {
        patrolDelay -= Time.deltaTime;
        if (patrolDelay < 0)
            canPatrol = true;

    }

    public override void ExitState()
    {
        Debug.Log("대기 상태에서 벗어남");
    }

}
