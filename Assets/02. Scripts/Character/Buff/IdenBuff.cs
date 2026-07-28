using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdenBuff : BuffBase
{
    protected CharacterModel model;

    public IdenBuff(CharacterModel model, BuffSO buffSO, float remainSecond) : base(buffSO, remainSecond)
    {
        this.model = model;
        return;
    }

    public override void OnEnable()
    {
        model.IdenEnable();
    }

    public override void OnDisable()
    {
        model.IdenDisable();
    }
}
