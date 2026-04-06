using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatBuff : BuffBase
{
    protected CharacterModel model;
    public C_Enums.CharacterStat effectedStat;
    public bool isFlat;
    public float value;

    public StatBuff(CharacterModel model,BuffSO buffSO, float remainSecond, C_Enums.CharacterStat effectedStat, bool isFlat, float value) : base(buffSO, remainSecond)
    {
        this.model = model;
        buffType = EBuffType.StatBuff;
        this.remainSecond = remainSecond;

        this.effectedStat = effectedStat;
        this.isFlat = isFlat;
        this.value = value;

        return;
    }

    public override void OnEnable()
    {
        model.AddStat(effectedStat, isFlat, value);
    }

    public override void OnDisable()
    {
        model.RemoveStat(effectedStat, isFlat, value);
    }

    
}
