using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImmunityBuff : BuffBase
{
    protected CharacterModel model;

    public ImmunityBuff(CharacterModel model, BuffSO buffSO, float remainSecond) : base(buffSO,remainSecond)
    {
        this.model = model;
        buffType = EBuffType.Immunity;
        this.remainSecond = remainSecond;

        return;
    }

    public override void OnEnable()
    {
        model.ImmunityEnable();
    }

    public override void OnDisable()
    {
        model.ImmunityDisable();
    }
}
