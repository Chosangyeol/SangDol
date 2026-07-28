using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvincibilityBuff : BuffBase
{
    protected CharacterModel model;

    public InvincibilityBuff(CharacterModel model, BuffSO buffSO, float remainSecond) : base(buffSO, remainSecond)
    {
        this.model = model;
        buffType = EBuffType.Invincibility;
        this.remainSecond = remainSecond;

        return;
    }

    public override void OnEnable()
    {
        model.InvincibilityEnable();
    }

    public override void OnDisable()
    {
        model.InvincibilityDisable();
    }
}
