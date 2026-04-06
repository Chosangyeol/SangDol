using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossModel : EnemyBase
{
    [Header("기믹 / 무력화 상태")]
    public bool isDoingSpecial = false;
    public bool isKnockDown = false;

    private List<BossPatternBase> normalPattenrs = new List<BossPatternBase>();
    private BossPatternBase currentPattern = null;



    protected override void Start()
    {
        base.Start();


    }

}
