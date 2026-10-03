using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PumpkinGay : EnemyModel
{
    public override void Attack()
    {
        base.Attack();
        PerformBoxMeleeAttack(1.2f, 1f, 1f);
    }
}
