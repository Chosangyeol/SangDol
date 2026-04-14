using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanicDeBuff : BuffBase
{
    protected CharacterModel model;

    public PanicDeBuff(CharacterModel model, BuffSO buffSO, float remainSecond) : base(buffSO,remainSecond)
    {
        this.model = model;
        buffType = EBuffType.Panic;
        this.remainSecond = remainSecond;
    }

    public override void OnEnable()
    {
        model.PanicEnable();
        model.StunEnable();
    }

    public override void OnDisable()
    {
        model.StunDisable();
        model.PanicDisable();
    }
}
