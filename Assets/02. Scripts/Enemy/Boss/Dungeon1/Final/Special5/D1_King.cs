using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class D1_King : BossModel
{
    private D1_Chess parent;

    public void Init(D1_Chess d1)
    {
        this.parent = d1;
    }

    protected override void Start()
    {
        base.Start();

        normalPatterns.Add(new D1_King_Stand());
    }

    protected override void Die(GameObject source = null)
    {
        parent.isDoing = false;
        parent.isClear = true;
        parent.ReturnBackStage();
    }
}

public class D1_King_Stand : BossPatternBase
{
    public D1_King_Stand()
    {
        patternName = "Stand";
        cooldown = 1f;
        weight = 1f;
        range = 10f;
    }

    public override void Execute(BossModel boss)
    {
        base.Execute(boss);

    }
}
