using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackDelayState : State
{
    public float attackDelay = 0f;
    public float timer = 0f;
    public AttackDelayState(EnemyModel owner) : base(owner)
    {
        attackDelay = owner.Stat.attackSpeed;
    }

    public override void EnterState()
    {
        timer = attackDelay;
    }

    public override void UpdateState()
    {
        timer -= attackDelay;

        if (timer <= 0f)
        {
            _owner.StateMachine.ChangeState(new AttackState(_owner));
        }
    }

    public override void ExitState()
    {
        
    }
}
